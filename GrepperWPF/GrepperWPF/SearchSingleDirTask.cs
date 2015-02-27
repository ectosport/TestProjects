using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GrepperWPF
{
   internal class SearchSingleDirTask
   {
      private readonly string _baseDir;
      private readonly int _baseDirLength;
      private readonly List<string> _fileExtensionsList = new List<string>();
      private readonly string _searchString;
      private readonly bool _searchFilenameOnly;
      private ConcurrentBag<SearchResult> _matches = new ConcurrentBag<SearchResult>();
      private CancellationTokenSource _cts;
      private bool _anyExtension = false;
      private int _filesFound;

      public List<SearchResult> SearchResults
      {
         get { return _matches.ToList(); }
      }

      public int FilesSearched
      {
         get { return _filesFound; }
      }

      public SearchSingleDirTask(string baseDir, string fileExtStr, string searchString, bool searchFilenameOnly)
      {
         _filesFound = 0;
         _baseDir = baseDir;
         _baseDirLength = baseDir.Length;
         _searchString = searchString;
         _searchFilenameOnly = searchFilenameOnly;
         _cts = new CancellationTokenSource();

         string[] fileExtensionArray = fileExtStr.Split(';');
         _anyExtension = (fileExtStr.Contains(".*") || String.IsNullOrEmpty(fileExtStr));

         // Check that each file extension begins with .            
         foreach (string s in fileExtensionArray)
         {
            string newStr = s;
            if (!s.StartsWith("."))
            {
               newStr = "." + s;
            }
            _fileExtensionsList.Add(newStr.ToLower());
         }
      }

      public void CancelSearch()
      {
         _cts.Cancel();
      }

      public Task PerformSearch()
      {
          DirectoryInfo dirInfo = new DirectoryInfo(_baseDir);

          return Task.Factory.StartNew(() =>
              {
                  recurseDirectory(_baseDir);
              }, TaskCreationOptions.AttachedToParent);
      }

      private void recurseDirectory(string dir)
      {
         DirectoryInfo dirInfo = new DirectoryInfo(dir);
         int numFiles = 0;

         try
         {
            foreach (FileInfo fileInfo in dirInfo.GetFiles())
            {
               _cts.Token.ThrowIfCancellationRequested();

               if (_anyExtension || _fileExtensionsList.Contains(fileInfo.Extension.ToLower()))
               {
                  ++numFiles;
                  if (_searchFilenameOnly)
                  {
                     if (fileInfo.Name.Contains(_searchString))
                     {
                        SearchResult sr = new SearchResult() { Filename = fileInfo.Name, RelativePath = (_baseDirLength > fileInfo.DirectoryName.Length ? "." : fileInfo.DirectoryName.Substring(_baseDirLength)), Path = fileInfo.DirectoryName };
                        _matches.Add(sr);
                     }
                  }
                  else
                  {
                      Task.Factory.StartNew(() => search(fileInfo), TaskCreationOptions.AttachedToParent);                       
                  }
               }
            }
         }
         catch (UnauthorizedAccessException e)
         {
            System.Diagnostics.Trace.WriteLine("*** Exception: " + e.ToString());
         }

         Interlocked.Add(ref _filesFound, numFiles);

         // recurse subdirectories
         try
         {
            foreach (DirectoryInfo childDir in dirInfo.GetDirectories())
            {
                _cts.Token.ThrowIfCancellationRequested();
               Task.Factory.StartNew(() => recurseDirectory(childDir.FullName), TaskCreationOptions.AttachedToParent);
            }
         }
         catch (UnauthorizedAccessException e)
         {
            System.Diagnostics.Trace.WriteLine("*** Exception: " + e.ToString());
         }
      }

      private void search(FileInfo data)
      {
            string contents;
            using (StreamReader reader = new StreamReader(data.FullName))
            {
                while ((contents = reader.ReadLine()) != null)
                {
                    if (contents.Contains(_searchString))
                    {
                        SearchResult sr = new SearchResult() { Filename = data.Name, RelativePath = (_baseDirLength > data.DirectoryName.Length ? "." : data.DirectoryName.Substring(_baseDirLength)), Path = data.DirectoryName };
                        _matches.Add(sr);
                        break;
                    }
                }
            }              
      }
   }

   internal class SearchResult
   {
      public string Filename
      {
         get;
         set;
      }

      public string Path
      {
         get;
         set;
      }

      public string RelativePath
      {
         get;
         set;
      }
   }
}
