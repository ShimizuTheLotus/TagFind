using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace TagFind.Classes.DataTypes
{
    public class T_StorageItem : INotifyPropertyChanged
    {
        public object? Item
        {
            get
            {
                if (IsStorageFile is null)
                {
                    return null;
                }
                if (IsStorageFile == true)
                {
                    return fileInstant;
                }
                else
                {
                    return folderInstant;
                }
            }
            set
            {
                if (value is null)
                {
                    fileInstant = null;
                    folderInstant = null;
                    IsStorageFile = null;
                }
                else if (value is StorageFile file)
                {
                    fileInstant = file;
                    folderInstant = null;
                    IsStorageFile = true;
                }
                else if (value is StorageFolder folder)
                {
                    fileInstant = null;
                    folderInstant = folder;
                    IsStorageFile = false;
                }
                else
                {
                    throw new InvalidCastException(message:$"Cannot cast {value.GetType()} to {typeof(T_StorageItem)}.");
                }
                OnPropertyChanged();
            }
        }

        public bool? IsStorageFile { get; private set; } = null;

        private StorageFile? fileInstant;
        private StorageFolder? folderInstant;

        public event PropertyChangedEventHandler? PropertyChanged;

        public T_StorageItem() { }
        public T_StorageItem(StorageFolder folder)
        {
            Item = folder;
        }
        public T_StorageItem(StorageFile file)
        {
            Item = file;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
