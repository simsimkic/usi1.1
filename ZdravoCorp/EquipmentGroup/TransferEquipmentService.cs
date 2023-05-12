﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ZdravoCorp.InfrastructureGroup;

namespace ZdravoCorp.EquipmentGroup
{
    public class TransferEquipmentService
    {

        public static List<AlteredEquipmentQuantity> GetSelectedItems(ItemCollection gridItems)
        {
            List<AlteredEquipmentQuantity> itemsForTransfer = new List<AlteredEquipmentQuantity>();
            foreach (AlteredEquipmentQuantity item in gridItems)
            {
                if (item.GetSelectedQuantity() != 0)
                {
                    itemsForTransfer.Add(item);
                }

            }
            return itemsForTransfer;
        }

        public static bool SaveStaticTransferRequest(ItemCollection gridItems, bool fromWarehouse, string roomFrom, string roomTo, DateTime transferDate)
        {
            List<AlteredEquipmentQuantity> itemsForTransfer = GetSelectedItems(gridItems);

            if (itemsForTransfer.Count > 0)
            {
                StaticEquipmentTransferRequest request = new StaticEquipmentTransferRequest(false, fromWarehouse, roomFrom, roomTo, transferDate, itemsForTransfer);

                StaticEquipmentTransferRequestRepository.Save(request);
                return true;

            }
            return false;
        }

        public static bool RemoveFromWarehouse(ref Dictionary<string, int> inventory, AlteredEquipmentQuantity equipmentQuantity, ref int howManyMoved)
        {
            string equipmentName = equipmentQuantity.GetName();
            int transferAmount = equipmentQuantity.GetSelectedQuantity();

            if (inventory.ContainsKey(equipmentName))
            {
                if (inventory[equipmentName] >= transferAmount)
                {
                    howManyMoved = transferAmount;
                    inventory[equipmentName] -= howManyMoved;
                    if (inventory[equipmentName] == 0)
                    {
                        inventory.Remove(equipmentName);
                    }
                    return true;
                }
                else
                {
                    howManyMoved = inventory[equipmentName];
                    equipmentQuantity.SetSelectedQuantity(transferAmount - howManyMoved);
                    inventory.Remove(equipmentName);
                }

            }

            return false;
        }

        public static bool RemoveFromRoom(ref List<FunctionalItem> functionalItems, AlteredEquipmentQuantity equipmentQuantity, ref int howManyMoved, string roomFrom)
        {
            string equipmentName = equipmentQuantity.GetName();
            int transferAmount = equipmentQuantity.GetSelectedQuantity();

            foreach (FunctionalItem item in functionalItems)
            {
                if (item.GetWhere() == roomFrom && item.GetWhat() == equipmentName)
                {
                    if (item.GetAmount() >= transferAmount)
                    {
                        howManyMoved = transferAmount;
                        item.SetAmount(item.GetAmount() - howManyMoved);
                        if (item.GetAmount() == 0)
                        {
                            functionalItems.Remove(item);
                        }
                        return true;
                    }
                    else
                    {
                        howManyMoved = item.GetAmount();
                        equipmentQuantity.SetSelectedQuantity(transferAmount - howManyMoved);
                        functionalItems.Remove(item);
                    }
                    break;
                }
            }
            return false;
        }

        public static void InsertIntoRoom(ref List<FunctionalItem> functionalItems, AlteredEquipmentQuantity equipmentQuantity, int howManyMoved, string roomTo)
        {

            string equipmentName = equipmentQuantity.GetName();

            bool found = false;

            foreach (FunctionalItem item in functionalItems)
            {
                if (item.GetWhere() == roomTo && item.GetWhat() == equipmentName)
                {
                    found = true;
                    item.SetAmount(item.GetAmount() + howManyMoved);
                    break;
                }
            }
            if (!found)
            {
                functionalItems.Add(new FunctionalItem(roomTo, equipmentName, howManyMoved));
            }
        }

        public static void MoveAllStaticEquipment(object state)
        {
            bool anyRequestsChanged = false;

            Warehouse warehouse = WarehouseRepository.Load();
            Dictionary<string, int> inventory = warehouse.GetInventory();

            List<FunctionalItem> functionalItems = FunctionalItemRepository.LoadAll();
            List<StaticEquipmentTransferRequest> allRequests = StaticEquipmentTransferRequestRepository.LoadAll();

            foreach (StaticEquipmentTransferRequest request in allRequests)
            {
                if (!request.IsFinished() && request.GetTransferDate() <= DateTime.Now)
                {
                    string roomFrom = request.GetRoomFrom();
                    string roomTo = request.GetRoomTo();

                    foreach (AlteredEquipmentQuantity equipmentQuantity in request.GetItemsForTransfer())
                    {

                        int howManyMoved = 0;

                        if (request.IsFromWarehouse())
                        {
                            bool isFinished = RemoveFromWarehouse(ref inventory, equipmentQuantity, ref howManyMoved);
                            if (isFinished)
                            {
                                request.Finish();
                            }
                        }
                        else
                        {
                            bool isFinished = RemoveFromRoom(ref functionalItems, equipmentQuantity, ref howManyMoved, roomFrom);
                            if (isFinished)
                            {
                                request.Finish();
                            }

                        }

                        if (howManyMoved > 0)
                        {
                            InsertIntoRoom(ref functionalItems, equipmentQuantity, howManyMoved, roomTo);
                            anyRequestsChanged = true;
                        }
                    }
                }
            }
            if (anyRequestsChanged)
            {
                StaticEquipmentTransferRequestRepository.SaveAll(allRequests);
                FunctionalItemRepository.SaveAll(functionalItems);
                WarehouseRepository.Save(warehouse);
            }
        }

        public static bool MoveDynamicEquipment(ItemCollection gridItems, bool fromWarehouse, string roomFrom, string roomTo)
        {
            List<AlteredEquipmentQuantity> itemsForTransfer = GetSelectedItems(gridItems);

            if (itemsForTransfer.Count > 0)
            {
                List<FunctionalItem> functionalItems = FunctionalItemRepository.LoadAll();

                if (fromWarehouse)
                {
                    Warehouse warehouse = WarehouseRepository.Load();
                    Dictionary<string, int> inventory = warehouse.GetInventory();

                    foreach (AlteredEquipmentQuantity equipmentQuantity in itemsForTransfer)
                    {

                        int howManyMoved = 0;
                        RemoveFromWarehouse(ref inventory, equipmentQuantity, ref howManyMoved);
                        InsertIntoRoom(ref functionalItems, equipmentQuantity, howManyMoved, roomTo);

                    }

                    WarehouseRepository.Save(warehouse);
                }

                else
                {
                    foreach (AlteredEquipmentQuantity equipmentQuantity in itemsForTransfer)
                    {

                        int howManyMoved = 0;
                        RemoveFromRoom(ref functionalItems, equipmentQuantity, ref howManyMoved, roomFrom);
                        InsertIntoRoom(ref functionalItems, equipmentQuantity, howManyMoved, roomTo);

                    }
                }
                FunctionalItemRepository.SaveAll(functionalItems);
                return true;
            }
            return false;

        }
    }
}
