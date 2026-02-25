using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SwiftJot.Models;

[JsonSerializable(typeof(List<Note>))]
[JsonSerializable(typeof(AppSettings))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
