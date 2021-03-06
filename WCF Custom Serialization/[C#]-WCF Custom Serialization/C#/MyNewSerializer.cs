﻿using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace OptimizedSerialization
{
    public class MyNewSerializer : XmlObjectSerializer
    {
        const string MyPrefix = "new";
        Type type;
        bool isCustomSerialization;
        XmlObjectSerializer fallbackSerializer;
        public MyNewSerializer(Type type, XmlObjectSerializer fallbackSerializer)
        {
            this.type = type;
            this.isCustomSerialization = typeof(ICustomSerializable).IsAssignableFrom(type);
            this.fallbackSerializer = fallbackSerializer;
        }

        public override bool IsStartObject(XmlDictionaryReader reader)
        {
            if (this.isCustomSerialization)
            {
                return reader.LocalName == MyPrefix;
            }
            else
            {
                return this.fallbackSerializer.IsStartObject(reader);
            }
        }

        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
        {
            if (this.isCustomSerialization)
            {
                object result = Activator.CreateInstance(this.type);
                MemoryStream ms = new MemoryStream(reader.ReadElementContentAsBase64());
                ((ICustomSerializable)result).InitializeFrom(ms);
                return result;
            }
            else
            {
                return this.fallbackSerializer.ReadObject(reader, verifyObjectName);
            }
        }

        public override void WriteEndObject(XmlDictionaryWriter writer)
        {
            if (this.isCustomSerialization)
            {
                writer.WriteEndElement();
            }
            else
            {
                this.fallbackSerializer.WriteEndObject(writer);
            }
        }

        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            if (this.isCustomSerialization)
            {
                MemoryStream ms = new MemoryStream();
                ((ICustomSerializable)graph).WriteTo(ms);
                byte[] bytes = ms.ToArray();
                writer.WriteBase64(bytes, 0, bytes.Length);
            }
            else
            {
                this.fallbackSerializer.WriteObjectContent(writer, graph);
            }
        }

        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
        {
            if (this.isCustomSerialization)
            {
                writer.WriteStartElement(MyPrefix);
            }
            else
            {
                this.fallbackSerializer.WriteStartObject(writer, graph);
            }
        }
    }
}
