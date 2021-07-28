﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using TAO3.Converters.Sql;
using TAO3.TypeProvider;

namespace TAO3.Converters.Sql
{
    public class SqlConverterParameters : ConverterCommandParameters
    {
        public string? Type { get; set; }
    }

    public class SqlConverter : 
        IConverter,
        IHandleCommand<Unit, SqlConverterParameters>
    {
        private readonly ITypeProvider<string> _typeProvider;
        private readonly SqlDeserializer _deserializer;
        private readonly ISqlObjectSerializer _serializer;

        public string Format => "sql";
        public string DefaultType => "dynamic";

        public SqlConverter(
            ITypeProvider<string> typeProvider, 
            SqlDeserializer deserializer, 
            ISqlObjectSerializer serializer)
        {
            _typeProvider = typeProvider;
            _deserializer = deserializer;
            _serializer = serializer;
        }

        public string Serialize(object? value)
        {
            return _serializer.Serialize(value);
        }

        public object? Deserialize<T>(string text)
        {
            return ((dynamic)_deserializer).Deserialize<T>(text);
        }

        public void Configure(Command command)
        {
            command.Add(new Option(new[] { "-t", "--type" }, "The type that will be use to deserialize the input text"));
        }

        public async Task HandleCommandAsync(IConverterContext<Unit> context, SqlConverterParameters args)
        {
            string sql = await context.GetTextAsync();
            string textVariableName = await context.CreatePrivateVariableAsync(sql, typeof(string));

            if (args.Type == null)
            {
                SchemaSerialization schema = _typeProvider.ProvideTypes(sql);

                await context.SubmitCodeAsync($@"{schema.Code}

{schema.RootType} {args.Name} = TAO3.Prelude.FromSql<{schema.ElementType}>({textVariableName});");
            }
            else
            {
                await context.SubmitCodeAsync($@"{args.Type} {args.Name} = TAO3.Prelude.FromSql<{args.Type}>({textVariableName});");
            }

        }
    }
}
