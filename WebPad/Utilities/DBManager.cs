using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebPad.Models;

namespace WebPad.Utilities
{
    public static class DBManager
    {
        private static nac.Database.SQLite.Database __internalDatabaseRef;

        public static nac.Database.SQLite.Database db
        {
            get
            {
                if( __internalDatabaseRef == null)
                {
                    setupDatabase();
                }

                return __internalDatabaseRef;
            }
        }

        private static void setupDatabase()
        {
            __internalDatabaseRef = new nac.Database.SQLite.Database(Properties.Settings.Default.DatabaseFilePath);

            __internalDatabaseRef.Command(@"
                create table if not exists RecentFiles(
                    FileName varchar(200) not null,
                    FullPath varchar(2000) not null
                )
            ");
            //__internalDatabaseRef.Command(@"drop table HtmlSnippets");
            __internalDatabaseRef.Command(@"
                create table if not exists HtmlSnippets(
                    BaseFilePath varchar(2000) not null,
                    FilePath varchar(2000) not null,
                    FileName varchar(2000) not null
                )
            ");
        }
        


        public static IEnumerable<Models.RecentFileModel> GetAllRecentFiles()
        {
            var recentEntries = db.Query(@"
                    select *
                    from RecentFiles
                ");

            var files = recentEntries.Select(dict => new Models.RecentFileModel
            {
                FileName = dict["FileName"] as string,
                Path = dict["FullPath"] as string
            });

            return files;
        }


        public static IEnumerable<Models.RecentHtmlSnippet> GetAllRecentHtmlSnippets()
        {
            var entries = db.Query(@"
                select *
                from HtmlSnippets
            ");

            return entries.Select(dict => RecentHtmlSnippetRowToModel(dict));
        }

        private static Models.RecentHtmlSnippet RecentHtmlSnippetRowToModel(Dictionary<string,object> dict)
        {
            return new Models.RecentHtmlSnippet
            {
                BaseFilePath = dict["BaseFilePath"] as string,
                FilePath = dict["FilePath"] as string,
                FileName = dict["FileName"] as string
            };
        }


        public static IEnumerable<Models.RecentHtmlSnippet> GetAllRecentHTMLSnippetsForBasePath(string basePath)
        {
            var entries = db.Query(@"
                select *
                from HtmlSnippets
                where LOWER(BaseFilePath) = :base
            ", new Dictionary<string, object>
            {
                {":base", basePath.ToLower() }
            });

            return entries.Select(dict => RecentHtmlSnippetRowToModel(dict));
        }


        public static void RenameRecentFile(Models.RecentFileModel file, string newName)
        {
            db.Command(@"
                    update RecentFiles
                    set FileName = :name
                    where LOWER(FullPath) = :path
                ", new Dictionary<string, object>
                {
                    { ":name", newName },
                    {":path", file.Path.ToLower() }
                });
        }



        public static bool AddRecentFileIfNotDuplicate(Models.RecentFileModel file)
        {
            bool didAdd = false;

            var existingEntryResult = db.Query(@"
                    select *
                    from RecentFiles
                    where LOWER(FullPath) = :path
                ", new Dictionary<string, object>
                {
                    {":path", file.Path.ToLower() }
                });

            if (!existingEntryResult.Any())
            {
                db.Command(@"
                    insert into RecentFiles(FileName, FullPath)
                    values(:name, :path)
                    ", new Dictionary<string, object>
                    {
                        {":name", file.FileName },
                        {":path", file.Path }
                    });
                didAdd = true;
            }

            return didAdd;
        }




        public static bool AddRecentHtmlSnippetIfNotDuplicate(Models.RecentHtmlSnippet snippet)
        {
            bool didAdd = false;

            var existingEntry = db.Query(@"
                select *
                from HtmlSnippets
                where LOWER(baseFilePath) = :base AND
                        LOWER(FilePath) = :path
            ", new Dictionary<string, object>
            {
                {":base", snippet.BaseFilePath.ToLower() },
                {":path", snippet.FilePath.ToLower() }
            });

            if(!existingEntry.Any())
            {
                db.Command(@"
                    insert into HtmlSnippets(baseFilePath, filePath, FileName)
                    values(:base,:path,:name)
                ", new Dictionary<string, object>
                {
                    {":base", snippet.BaseFilePath },
                    {":path", snippet.FilePath },
                    {":name", snippet.FileName }
                });
                didAdd = true;
            }

            return didAdd;
        }


        public static void RemoveRecentFile(RecentFileModel file)
        {
            db.Command(@"
                delete from RecentFiles
                where LOWER(FullPath) = :path
            ", new Dictionary<string, object>
            {
                { ":path", file.Path.ToLower() }
            });
        }

        public static void ClearAllRecentFiles()
        {
            // delete all the recent files
            db.Command(@"
                delete from RecentFiles
            ");
        }
    }
}
