﻿namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Exceptions;
    using Craftsman.Models;
    using Newtonsoft.Json;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using YamlDotNet.Serialization;
    using static ConsoleWriter;

    public static class ApiCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   Scaffolds out API files and projects based on a given template file in a json or yaml format.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman new:api [options] <filepath>{Environment.NewLine}");

            WriteHelpText(@$"   For example:");
            WriteHelpText(@$"       craftsman new:api C:\fullpath\api.yaml");
            WriteHelpText(@$"       craftsman new:api C:\fullpath\api.yml");
            WriteHelpText(@$"       craftsman new:api C:\fullpath\api.json{Environment.NewLine}");

            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message. No filepath is needed to display the help message.");
        }

        public static void Run(string filePath)
        {
            try
            {
                RunInitialGuards(filePath);
                var template = GetApiTemplateFromFile(filePath);
                WriteInfo($"The template file was parsed successfully.");

                RunTemplateGuards(template);

                //var rootProjectDirectory = Directory.GetCurrentDirectory().Contains("Debug") ? @"C:\Users\Paul\Documents\testoutput" : Directory.GetCurrentDirectory();
                var buildSolutionDirectory = @"C:\Users\Paul\Documents\testoutput";

                // create projects
                CreateNewFoundation(template, buildSolutionDirectory); // todo scaffold this manually instead of using dotnet new foundation
                var solutionDirectory = $"{buildSolutionDirectory}\\{template.SolutionName}";

                //entities
                foreach (var entity in template.Entities)
                {
                    EntityDtoBuilder.CreateEntity(solutionDirectory, entity);
                    EntityDtoBuilder.CreateDtos(solutionDirectory, entity);
                }

                WriteInfo($"The API command was successfully completed.");
            }
            catch(Exception e)
            {
                if(e is FileAlreadyExistsException 
                    || e is DirectoryAlreadyExistsException 
                    || e is InvalidSolutionNameException 
                    || e is FileNotFoundException 
                    || e is InvalidFileTypeException)
                {
                    WriteError($"{e.Message}");
                }
                else
                    WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
            }
        }

        private static void RunTemplateGuards(ApiTemplate template)
        {
            if(template.SolutionName == null || template.SolutionName.Length <= 0)
            {
                throw new InvalidSolutionNameException();
            }
        }

        private static void CreateNewFoundation(ApiTemplate template, string directory)
        {
            var newDir = $"{directory}\\{template.SolutionName}";
            if (Directory.Exists(newDir))
                throw new DirectoryAlreadyExistsException(newDir);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = @$"new foundation -n {template.SolutionName} -e ""Recipe"" -en ""recipe"" -la ""r""",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = directory
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private static ApiTemplate GetApiTemplateFromFile(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            if (ext == ".yml" || ext == ".yaml")
                return ReadYaml(filePath);
            else
                return ReadJson(filePath);
        }

        public static ApiTemplate ReadYaml(string yamlFile)
        {
            var deserializer = new Deserializer();
            ApiTemplate templatefromYaml = deserializer.Deserialize<ApiTemplate>(File.ReadAllText(yamlFile));

            return templatefromYaml;
        }
        public static ApiTemplate ReadJson(string jsonFile)
        {
            return JsonConvert.DeserializeObject<ApiTemplate>(File.ReadAllText(jsonFile));

            // deserialize JSON directly from a file
            /*using (StreamReader file = File.OpenText(jsonFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                var deserializedTemplate2 = (ApiTemplate)serializer.Deserialize(file, typeof(ApiTemplate));
            }*/
        }

        public static bool RunInitialGuards(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            if (!IsJsonOrYaml(filePath))
            {
                //TODO: add link to docs for examples
                throw new InvalidFileTypeException();
            }

            return true;
        }

        public static bool IsJsonOrYaml(string filePath)
        {
            var validExtensions = new string[] { ".json", ".yaml", ".yml" };
            return validExtensions.Contains(Path.GetExtension(filePath));
        }
    }
}