using Fraud.Contracts;
using System.Text.Json.Serialization;

namespace Fraud.Scoring.Infrastructure
{
    [JsonSerializable(typeof(TransactionCreated))]
    [JsonSerializable(typeof(List<TransactionCreated>))]
    internal partial class FraudJson : JsonSerializerContext;
}
