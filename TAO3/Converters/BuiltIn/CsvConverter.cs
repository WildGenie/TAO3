﻿using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.DotNet.Interactive;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TAO3.Internal.CodeGeneration.Generators;

namespace TAO3.Converters
{
    public class CsvConverter : IConverter<CsvConfiguration>,  IConfigurableConverter
    {
        private readonly CsvConfiguration _defaultSettings;

        private readonly bool _hasHeader;
        public string Format => _hasHeader ? "csvh" : "csv";

        public string DefaultType => "string[][]";

        public CsvConverter(bool hasHeader)
        {
            _hasHeader = hasHeader;
            _defaultSettings = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = _hasHeader
            };
        }

        public object? Deserialize<T>(string text, CsvConfiguration? settings)
        {
            bool isDynamic = typeof(T) == typeof(ExpandoObject);

            using StringReader reader = new StringReader(text);
            using CsvReader csvReader = new CsvReader(reader, settings ?? _defaultSettings);
            return isDynamic
                ? GetRows(csvReader).ToArray()
                : csvReader.GetRecords<T>().ToArray();
        }

        private IEnumerable<string[]> GetRows(CsvReader csvReader)
        {
            while (csvReader.Read())
            {
                yield return csvReader.Context.Record;
            }
        }

        public string Serialize(object? value, CsvConfiguration? settings)
        {
            if (value == null)
            {
                return string.Empty;
            }

            Type? enumerableType = GetGenericTypeUsage(value.GetType(), typeof(IEnumerable<>));
            IEnumerable<object> values = enumerableType != null
                ? (IEnumerable<object>)value!
                : new object[] { value! };

            using StringWriter textWriter = new StringWriter();
            using CsvWriter csvWriter = new CsvWriter(textWriter, settings ?? _defaultSettings);
            csvWriter.WriteRecords(values);

            return textWriter.ToString();
        }

        //https://stackoverflow.com/questions/5461295/using-isassignablefrom-with-open-generic-types
        private static Type? GetGenericTypeUsage(Type givenType, Type genericType)
        {
            Type[] interfaceTypes = givenType.GetInterfaces();

            foreach (Type it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                {
                    return it;
                }
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return givenType;
            }

            Type? baseType = givenType.BaseType;
            if (baseType == null)
            {
                return null;
            }

            return GetGenericTypeUsage(baseType, genericType);
        }

        public void ConfigureCommand(Command command, ConvertionContextProvider contextProvider)
        {
            command.Add(new Option<string>(new[] { "-s", "--separator" }, "Value separator"));
            command.Add(new Option(new[] { "-t", "--type" }, "The type that will be use to deserialize the input text"));

            command.Handler = CommandHandler.Create(async (string source, string name, string settings, bool verbose, string separator, string type, KernelInvocationContext context) =>
            {
                IConverterContext<CsvConfiguration> converterContext = contextProvider.Invoke<CsvConfiguration>(source, name, settings, verbose, context);

                converterContext.Settings ??= _defaultSettings;

                if (!string.IsNullOrEmpty(separator))
                {
                    converterContext.Settings.Delimiter = Regex.Unescape(separator);
                }

                if (type == "dynamic")
                {
                    await converterContext.DefaultHandle();
                    return;
                }

                if (string.IsNullOrEmpty(type))
                {
                    string code = await new CsvCodeGenerator().GenerateSourceCodeAsync(converterContext);
                    await converterContext.SubmitCodeAsync(code);
                }
                else
                {
                    string clipboardVariableName = await converterContext.CreatePrivateVariable(await converterContext.GetTextAsync(), typeof(string));
                    string converterVariableName = await converterContext.CreatePrivateVariable(converterContext.Converter, typeof(CsvConverter));
                    string settingsVariableName = await converterContext.CreatePrivateVariable(converterContext.Settings, typeof(CsvConfiguration));

                    string code = $"{type}[] {name} = ({type}[]){converterVariableName}.Deserialize<{type}>({clipboardVariableName}, {settingsVariableName});";
                    await converterContext.SubmitCodeAsync(code);

                }
            });

        }
    }
}
