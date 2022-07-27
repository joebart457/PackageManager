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
				.ShowHelpIfUnresolvable()
				.Group("install")
					.Verb()
					.Description("the name of the package to install, filepath of the local package manifest, or remote url of the package manifest")
					.Option("l", "latest")
						.WithDescription("use 'latest' as the tag value")
						.WithValidator(s => s is not null)
						.IsFlag()
					.Option("t", "tag")
						.WithDescription("the tag value of the package name. cannot be used when filepath or remote url is specified")
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
				.Group("describe")
					.SubGroup(
						CommandBuilder.CommandGroup("package")
							.Verb()
							.Description("the name of the package, filepath of local package manifest, or remote url of the package manifest")
							.Option("t", "tag")
								.WithDescription("used together with the package name to determine which package to describe. cannot be used when filepath or remote url is specified")
								.WithDefault("")
							.Action(args =>
                            {
								string uri = args.Verb<string>();
								string tag = args.ValueOf("t", "tag");
								if (string.IsNullOrEmpty(tag))
                                {
									var t = PackageManagerService.DescribePackage(uri);
									t.Wait();
                                } else
                                {
									var t = PackageManagerService.DescribePackage(uri, tag);
									t.Wait();
                                }
								return 0;
                            })
					)
				.Group("rm")
					.Verb()
					.Option("t", "tag")
						.Required()
					.Action(args =>
                    {
						var t = PackageManagerService.DeleteManifestFromRemote(args.Verb<string>(), args.ValueOf("t", "tag"));
						t.Wait();
						return 0;
                    })
				.Group("ls")
					.Verb("packages")
					.Description("lists all available packages")
					.Action(args =>
					{
						var task = PackageManagerService.ListAllPackages();
						task.Wait();
						return 0;
					})
					.Verb("remote")
					.Description("displays the current configured remote package server")
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
							.Description("lists all tags for the specified package name")
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
					.Description("uploads the local (or remote) package to the currently configured remote package server")
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
							.Description("configures the remote url")
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
				main.Execute(args);
			}
			catch (Exception ex)
            {
				Console.WriteLine(ex);
            }
		}
	}
}