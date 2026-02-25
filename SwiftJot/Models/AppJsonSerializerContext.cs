using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SwiftJot.Models;

[JsonSerializable(typeof(List<Note>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
