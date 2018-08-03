using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.Utilities
{
    public static class DBManager
    {
        private static netstandardDbSQLiteHelper.Database __internalDatabaseRef;

        public static netstandardDbSQLiteHelper.Database db
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
            __internalDatabaseRef = new netstandardDbSQLiteHelper.Database(Properties.Settings.Default.DatabaseFilePath);

            __internalDatabaseRef.Command(@"
                create table if not exists RecentFiles(
                    FileName varchar(200) not null,
                    FullPath varchar(2000) not null,
                    Type varchar(100) not null
                )
            ");
            __internalDatabaseRef.Command(@"
                create table if not exists HtmlSnippets(
                    BaseFilePath varchar(2000) not null,
                    FilePath varchar(2000) not null
                )
            ");
        }


        private static File.SaveHandler.SaveType GetRecentFileTypeFromString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return File.SaveHandler.SaveType.WebPad;
            }

            return (File.SaveHandler.SaveType)Enum.Parse(typeof(File.SaveHandler.SaveType), text);
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
                Path = dict["FullPath"] as string,
                Type = GetRecentFileTypeFromString(dict["Type"] as string)
            });

            return files;
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
                    insert into RecentFiles(FileName, FullPath, Type)
                    values(:name, :path, :type)
                    ", new Dictionary<string, object>
                    {
                        {":name", file.FileName },
                        {":path", file.Path },
                        {":type", file.Type.ToString() }
                    });
                didAdd = true;
            }

            return didAdd;
        }




    }
}
