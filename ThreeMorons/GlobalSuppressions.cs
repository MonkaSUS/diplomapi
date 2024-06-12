// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "ASP0022:Route conflict detected between route handlers", Justification = "Это веб-апи, потребление которого организую я. Я знаю все адреса и все методы к ним", Scope = "member", Target = "~M:ThreeMorons.Initialization.Initializer.MapSkippedClassEndpoints(Microsoft.AspNetCore.Builder.WebApplication)")]
[assembly: SuppressMessage("Usage", "ASP0022:Route conflict detected between route handlers", Justification = "Это веб-апи, потребление которого организую я. Я знаю все адреса и все методы к ним", Scope = "member", Target = "~M:ThreeMorons.Initialization.Initializer.MapUserEndpoints(Microsoft.AspNetCore.Builder.WebApplication,Microsoft.AspNetCore.Builder.WebApplicationBuilder)")]
[assembly: SuppressMessage("Usage", "ASP0022:Route conflict detected between route handlers", Justification = "Это веб-апи, потребление которого организую я. Я знаю все адреса и все методы к ним", Scope = "member", Target = "~M:ThreeMorons.Initialization.Initializer.MapDelayEndpoints(Microsoft.AspNetCore.Builder.WebApplication)")]
