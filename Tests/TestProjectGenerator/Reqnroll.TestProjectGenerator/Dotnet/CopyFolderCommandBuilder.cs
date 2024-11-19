﻿using System;
using System.IO;

namespace Reqnroll.TestProjectGenerator.Dotnet;
public class CopyFolderCommandBuilder : CommandBuilder
{
    private readonly string _replaceFrom;
    private readonly string _replaceTo;

    public CopyFolderCommandBuilder(IOutputWriter outputWriter, string sourceFolder, string targetFolder, string replaceFrom = null, string replaceTo = null) 
        : base(outputWriter, "[copy folder]", sourceFolder, targetFolder)
    {
        _replaceFrom = replaceFrom;
        _replaceTo = replaceTo;
    }

    private string ReplaceName(string value)
    {
        if (_replaceFrom == null || _replaceTo == null) return value;
        return value.Replace(_replaceFrom, _replaceTo);
    }

    private void CopyDirectoryRecursively(DirectoryInfo source, DirectoryInfo target)
    {
        Directory.CreateDirectory(target.FullName);

        // Copy each file into the new directory.
        foreach (FileInfo fi in source.GetFiles())
        {
            string fiName = ReplaceName(fi.Name);
            _outputWriter.WriteLine(@"Copying to {0}\{1}", target.FullName, fiName);
            fi.CopyTo(Path.Combine(target.FullName, fiName), true);
        }

        // Copy each subdirectory using recursion.
        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        {
            DirectoryInfo nextTargetSubDir =
                target.CreateSubdirectory(diSourceSubDir.Name);
            CopyDirectoryRecursively(diSourceSubDir, nextTargetSubDir);
        }
    }

    public override CommandResult Execute(Func<Exception, Exception> exceptionFunction)
    {
        var sourceFolder = ArgumentsFormat;
        var targetFolder = WorkingDirectory;

        try
        {
            _outputWriter.WriteLine($"Copying '{sourceFolder}' to '{targetFolder}'...");

            CopyDirectoryRecursively(new DirectoryInfo(sourceFolder), new DirectoryInfo(targetFolder));

            _outputWriter.WriteLine("Copying done.");

            return new CommandResult(0, $"Copied '{sourceFolder}' to '{targetFolder}'.");
        }
        catch (Exception ex)
        {
            throw exceptionFunction(ex);
        }
    }
}