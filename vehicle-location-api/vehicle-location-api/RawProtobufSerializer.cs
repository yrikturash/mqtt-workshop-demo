using System;
using System.IO;
using System.Net.Mime;
using System.Runtime.Serialization;
using Google.Protobuf;
using MassTransit;
using MassTransit.Serialization;
using ProtoBuf;

namespace vehicle_location_api
{
    public class RawProtobufSerializer : IMessageSerializer
    {
        public const string ContentTypeHeaderValue = "application/x-protobuf";
        public static readonly ContentType ProtocolBuffersContentType = new ContentType(ContentTypeHeaderValue);

        public void Serialize<T>(Stream stream, SendContext<T> context)
            where T : class
        {
            try
            {
                if (context.Message is Fault)
                {

                    var s = new RawJsonMessageSerializer();
                    context.ContentType = s.ContentType;
                    s.Serialize(stream, context);
                }
                else if (context.Message is IMessage message)
                {
                    var bytes = message.ToByteArray();
                    stream.Write(bytes, 0, bytes.Length);
                }
                else
                {
                    context.ContentType = ProtocolBuffersContentType;
                    Serializer.Serialize(stream, context.Message);
                }
            }
            catch (SerializationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SerializationException("Failed to serialize message", ex);
            }
        }

        public ContentType ContentType => ProtocolBuffersContentType;
    }
}