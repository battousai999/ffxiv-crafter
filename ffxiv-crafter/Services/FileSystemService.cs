using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ffxiv_crafter.Services
{
    public interface IFileSystemService
    {
        string GetSaveFilename();
        string GetOpenFilename();
        string ReadAllText(string filename);
        void WriteAllText(string filename, string data);
    }

    public class FileSystemService : IFileSystemService
    {
        public string GetSaveFilename()
        {
            var saveDialog = new SaveFileDialog();

            saveDialog.Filter = "Save-Data file(*.json)|*.json|All Files (*.*)|*.*";
            saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (!(saveDialog.ShowDialog() ?? false))
                return null;

            return saveDialog.FileName;
        }

        public string GetOpenFilename()
        {
            var openDialog = new OpenFileDialog();

            openDialog.Filter = "Save-Data file(*.json)|*.json|All Files (*.*)|*.*";
            openDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (!(openDialog.ShowDialog() ?? false))
                return null;

            return openDialog.FileName;
        }

        public string ReadAllText(string filename)
        {
            return File.ReadAllText(filename);
        }

        public void WriteAllText(string filename, string data)
        {
            File.WriteAllText(filename, data);
        }
    }
}
