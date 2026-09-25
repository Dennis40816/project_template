using ProjectTemplate.Bootstrap;
using ProjectTemplate.Cli;

return await CliApplication.RunAsync(args, CompositionRoot.Create(), Console.Out, Console.Error).ConfigureAwait(false);
