﻿using AndroidBackups.UI.Models;
using PortableDevicesLib;
using PortableDevicesLib.Domain;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace AndroidBackups.UI.ViewModels
{
    public class BackuperViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private IList<PortableDevice> _devices;
        public IList<PortableDevice> Devices
        {
            get { return _devices; }
            set
            {
                if (_devices != value)
                {
                    _devices = value;
                    OnPropertyChanged(nameof(Devices));
                }
            }
        }

        private PortableDevice _selectedDevice;
        public PortableDevice SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                if (_selectedDevice != value)
                {
                    _selectedDevice = value;                    
                    OnPropertyChanged(nameof(SelectedDevice));

                    InitTreeView();
                }
            }
        }

        private List<TreeViewItem> _rootFileItem;
        public List<TreeViewItem> FileItems
        {
            get { return _rootFileItem; }
            set
            {
                if (_rootFileItem != value)
                {
                    _rootFileItem = value;
                    OnPropertyChanged(nameof(FileItems));
                }
            }
        }

        private IList<SourceFolder> _sourceFolders;
        public IList<SourceFolder> SourceFolders
        {
            get { return _sourceFolders; }
            set
            {
                if (_sourceFolders != value)
                {
                    _sourceFolders = value;
                    OnPropertyChanged(nameof(SourceFolders));
                }
            }
        }

        private ICommand _openConfigurationFileCommand;
        public ICommand OpenConfigurationFileCommand { get { return _openConfigurationFileCommand; } }

        public BackuperViewModel()
        {
            // Get the only connected MTP device:                
            var service = new StandardPortableDevicesService();
            var devices = service.Devices;

            // Add "null" in front of them:
            var devicesForUi = new List<PortableDevice>() { null };
            foreach (var device in devices)
                devicesForUi.Add(device);

            this.Devices = devicesForUi;

            //InitSourceFolders();

            // Delegates:
            //_openConfigurationFileCommand = new DelegateCommand(() => _episodeRepository.OpenConfigurationFile());
        }

        private void InitTreeView()
        {
            _selectedDevice.Connect();
            var rootFolder = _selectedDevice.GetFullContents();
            _selectedDevice.Disconnect();

            var rootTreeViewItem = GetTreeViewItem(rootFolder);
            var subTreeViewItems = GetTreeViewItems(rootFolder);
            AddItemsToTreeViewItem(rootTreeViewItem, subTreeViewItems);

            this.FileItems = new List<TreeViewItem>() { rootTreeViewItem };
        }

        private List<TreeViewItem> GetTreeViewItems(PortableDeviceFolder portableDeviceFolder)
        {
            var treeViewItems = new List<TreeViewItem>();
            foreach (var element in portableDeviceFolder.Files)
            {
                var treeViewItem = GetTreeViewItem(element);
                if (element.GetType() == typeof(PortableDeviceFolder))
                {
                    var subFolder = (PortableDeviceFolder)element;
                    var subTreeViewItems = GetTreeViewItems(subFolder);
                    AddItemsToTreeViewItem(treeViewItem, subTreeViewItems);
                }
                treeViewItems.Add(treeViewItem);
            }
            return treeViewItems;
        }

        private TreeViewItem GetTreeViewItem(PortableDeviceObject portableDeviceObject)
        {
            var treeviewItem = new TreeViewItem();
            if (portableDeviceObject.GetType() == typeof(PortableDeviceFolder))
            {
                treeviewItem.Header = "\\" + portableDeviceObject.Name;
            }
            else
            {
                treeviewItem.Header = portableDeviceObject.Name;
            }
            return treeviewItem;
        }

        private void AddItemsToTreeViewItem(TreeViewItem treeViewItem, List<TreeViewItem> subTreeViewItems)
        {
            foreach (var subTreeViewItem in subTreeViewItems)
                treeViewItem.Items.Add(subTreeViewItem);
        }

        private void InitSourceFolders()
        {
            var souceFolderFullPaths = new string[] {
                @"This PC\XT1068\Internal storage\WhatsApp\Media\WhatsApp Images",
                @"This PC\XT1068\Internal storage\WhatsApp\Media\WhatsApp Video",
                @"This PC\XT1068\Internal storage\DCIM\Camera"
            };
            this.SourceFolders = new List<SourceFolder>();
            foreach (var sourceFolderFullPath in souceFolderFullPaths)
            {
                var sourceFolder = new SourceFolder(sourceFolderFullPath);
                this.SourceFolders.Add(sourceFolder);
            }
        }

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
