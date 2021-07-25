﻿namespace TAO3.TypeProvider
{
    public class ClassPropertySchema : ISchema
    {
        public string Identifier { get; }
        public string FullName { get; }
        public TypeReferenceSchema Type { get; }

        public ClassPropertySchema(string identifier, string fullName, TypeReferenceSchema type)
        {
            Identifier = identifier;
            FullName = fullName;
            Type = type;
        }

        public ClassPropertySchema WithType(TypeReferenceSchema type)
        {
            return new ClassPropertySchema(
                Identifier,
                FullName,
                type);
        }

        ISchema ISchema.Accept(SchemaRewriter rewriter) => Accept(rewriter);
        public ClassPropertySchema Accept(SchemaRewriter rewriter)
        {
            return rewriter.Visit(this);
        }

        public void Accept(SchemaVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
