﻿namespace Craftsman.Helpers
{
    using Craftsman.Enums;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;

    public static class ClassPathHelper
    {
        public static ClassPath EntityClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "Domain\\Entities", className);
        }
        public static ClassPath IRepositoryClassPath(string solutionDirectory, string className, string entityName)
        {
            return new ClassPath(solutionDirectory, $"Application\\Interfaces\\{entityName}", className);
        }

        public static ClassPath RepositoryClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, $"Infrastructure.Persistence\\Repositories", className);
        }

        public static ClassPath DtoClassPath(string solutionDirectory, string className, string entityName)
        {
            return new ClassPath(solutionDirectory, $"Application\\Dtos\\{entityName}", className);
        }
    }
}
