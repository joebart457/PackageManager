using cli.Builders;
using cli.Models;
using PM.Client;
using PM.Models.Enums;
using PM.Services;
using System;
using System.Linq;

namespace PM
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var main = CommandBuilder.CommandGroup("cpm")
				.Group("install")
					.Verb()
					.Option("l", "latest")
						.WithValidator(s => s is not null)
						.IsFlag()
					.Option("t", "tag")
						.WithValidator(s => s is not null)
						.WithDefault("")
					.Action(args =>
					{
						bool latest = args.Get<bool>("l", "latest");
						string tag = args.ValueOf("t", "tag");
						string name = args.Verb<string>();

						if (!latest && tag == "")
						{
							var task = PackageManagerService.GetPackageWithLogging(name);
							task.Wait();
						}
						else
						{
							var task = PackageManagerService.GetPackageWithLogging(name, latest ? "latest" : tag);
							task.Wait();
						}

						return 0;
					})
				.Group("ls")
					.Verb("packages")
					.Action(args =>
					{
						var task = PackageManagerService.ListAllPackages();
						task.Wait();
						return 0;
					})
					.Verb("remote")
					.Action(args =>
					{
						try
						{
							LoggerService.Log($"remote: {ConfigService.GetConfig("baseUrl")}");
						}
						catch (Exception ex)
						{
							LoggerService.Log($"Unexpected error occurred", LogSeverity.ERROR, true);
							LoggerService.Log($"ERROR -- {ex.Message}", LogSeverity.ERROR, true); ;
						}
						return 0;
					})
					.SubGroup(
						CommandBuilder.CommandGroup("tags")
							.Verb()
							.Action(args =>
							{
								string name = args.Verb<string>();
								var task = PackageManagerService.ListAllTags(name);
								task.Wait();
								return 0;
							})
					)
				.Group("push")
					.Verb()
					.Action(args =>
					{
						var t = PackageManagerService.PushManifestToRemote(args.Verb<string>());
						t.Wait();
						return 0;
					})
				.Group("change")
					.SubGroup(
						CommandBuilder.CommandGroup("remote")
							.Verb()
							.Action(args =>
                            {
                                try
                                {
									string baseUrl = args.Verb<string>();
									ConfigService.UpdateConfig("baseUrl", baseUrl);
									LoggerService.Log($"updated remote to {baseUrl}");
								}catch (Exception ex)
                                {
									LoggerService.Log($"Unexpected error occurred", LogSeverity.ERROR, true);
									LoggerService.Log($"ERROR -- {ex.Message}", LogSeverity.ERROR, true);
								}
								
								return 0;
                            })
				);
            try
            {
				main.Resolve(args);
			}
			catch (Exception ex)
            {
				Console.WriteLine(ex);
            }
		}
	}
}