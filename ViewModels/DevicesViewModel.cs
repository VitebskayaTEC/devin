using Dapper;
using Devin.Models;
using System.Collections.Generic;

namespace Devin.ViewModels
{
    public class DevicesViewModel
    {
        public List<Folder> Folders { get; set; } = new List<Folder>();

        public List<Computer> Computers { get; set; } = new List<Computer>();

        public List<Device> Devices { get; set; } = new List<Device>();

        public DevicesViewModel(string Search = "")
        {
            using (var conn = Database.Connection())
            {
                if (!string.IsNullOrEmpty(Search))
                {
                    Devices = conn.Query<Device>($@"SELECT
                        Devices.Inventory,
                        Devices.Type,
                        Devices.Id,
                        Devices.Name,
                        Devices.ComputerId,
                        Devices.PublicName,
                        Devices.Mol,
                        WorkPlaces.Location,
                        Devices.IsOff,
                        Folders.Id         AS FolderId
                    FROM Devices
                    LEFT OUTER JOIN Folders    ON Folders.Id  = Devices.FolderId
	                LEFT OUTER JOIN WorkPlaces ON WorkPlaces.Id = Devices.PlaceId
                    WHERE Devices.IsDeleted = 0 AND (
                           Devices.Inventory LIKE '%{Search}%'
                        OR Devices.Type LIKE '%{Search}%'
                        OR Devices.Name LIKE '%{Search}%'
                        OR Devices.PublicName LIKE '%{Search}%'
                        OR Devices.Description LIKE '%{Search}%'
                        OR Devices.Mol LIKE '%{Search}%'
                        OR Devices.SerialNumber LIKE '%{Search}%'
                        OR Devices.PassportNumber LIKE '%{Search}%'
                        OR Devices.Location LIKE '%{Search}%')
                    ORDER BY Name, Type").AsList();
                }
                else
                {
                    List<Device> __devices = conn.Query<Device>(@"SELECT
                        Devices.Inventory,
                        Devices.Type,
                        Devices.Id,
                        Devices.Name,
                        Devices.ComputerId,
                        Devices.PublicName,
                        Devices.Mol,
                        WorkPlaces.Location,
                        Devices.IsOff,
                        Folders.Id         AS FolderId
                    FROM Devices
                    LEFT OUTER JOIN Folders    ON Folders.Id  = Devices.FolderId
	                LEFT OUTER JOIN WorkPlaces ON WorkPlaces.Id = Devices.PlaceId
                    WHERE (Devices.IsDeleted = 0) ORDER BY Name, Type").AsList();
                    
                    List<Folder> _folders = conn.Query<Folder>(@"SELECT 
	                    Folders.Id,
	                    CASE WHEN Parents.Id IS NULL THEN 0 ELSE Parents.Id END AS FolderId,
	                    Folders.Name
                    FROM Folders
	                LEFT OUTER JOIN Folders AS Parents ON Folders.FolderId = Parents.Id
                    WHERE Folders.Type = 'device'
                    ORDER BY Folders.Name").AsList();

                    List<Device> _devices = new List<Device>();
                    List<Computer> _computers = new List<Computer>();

                    foreach (Device d in __devices)
                    {
                        if (d.Type == "CMP")
                        {
                            _computers.Add(new Computer
                            {
                                Id = d.Id,
                                Type = d.Type,
                                Inventory = d.Inventory,
                                Name = d.Name,
                                PublicName = d.PublicName,
                                Mol = d.Mol,
                                Location = d.Location,
                                IsOff = d.IsOff,
                                FolderId = d.FolderId
                            });
                        }
                        else
                        {
                            _devices.Add(d);
                        }
                    }
                    
                    bool found = false;

                    foreach (Device d in _devices)
                    {
                        found = false;
                        foreach (Computer computer in _computers)
                        {
                            if (d.ComputerId == computer.Id)
                            {
                                computer.Devices.Add(d);
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            foreach (Folder folder in _folders)
                            {
                                if (d.FolderId == folder.Id)
                                {
                                    folder.Devices.Add(d);
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                Devices.Add(d);
                            }
                        }
                    }
                    
                    foreach (Computer computer in _computers)
                    {
                        found = false;
                        foreach (Folder folder in _folders)
                        {
                            if (computer.FolderId == folder.Id)
                            {
                                folder.Computers.Add(computer);
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            Computers.Add(computer);
                        }
                    }
                    
                    foreach (Folder folder in _folders)
                    {
                        if (folder.FolderId == 0)
                        {
                            Folders.Add(Folder.Build(folder, _folders));
                        }
                    }
                }
            }
        }
    }
}