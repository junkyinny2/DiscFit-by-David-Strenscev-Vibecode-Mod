using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscFit
{
    struct BinItem : IComparable
    {
        public string displayName;
        public string itemPath;
        public ulong itemSize;

        public BinItem(string DisplayName, string Path, ulong Size)
        {
            this.displayName = DisplayName;
            this.itemPath = Path;
            this.itemSize = Size;
        }

        public int CompareTo(object obj)
        {
            BinItem binItem = (BinItem)obj;
            if (this.itemSize > binItem.itemSize)
                return 1;
            else if (this.itemSize < binItem.itemSize)
                return -1;
            else
                return 0;
        }

        public override string ToString()
        {
            return (String.Format("{0}:{1}", this.itemPath, this.itemSize));
        }
    }

    class Bin
    {
        public ulong size = 0;
        public List<BinItem> items = new List<BinItem>(); // Undefined length array

        public void Item_add(string itemDisplayName, string itemPath, ulong itemSize)
        {
            BinItem binItem = new BinItem(itemDisplayName, itemPath, itemSize);

            this.items.Add(binItem);
            this.size += itemSize;
        }
    }

    class BinPacker
    {
        private ulong binMaxSize = 0;
        private List<BinItem> list = new List<BinItem>();
        public List<BinItem> oversized = new List<BinItem>();
        public List<Bin> bins = new List<Bin>();

        public BinPacker(ulong binMaxSize)
        {
            this.binMaxSize = binMaxSize;
        }

        public void listAdd(BinItem binItem)
        {
            if (binItem.itemSize > binMaxSize)
                this.oversized.Add(binItem);
            else
                this.list.Add(binItem);
        }

        private void listSortDesc()
        {
            this.list.Sort();
            this.list.Reverse();
        }

        public void BestFit()
        {
            foreach (BinItem item in this.list)
            {
                if (item.itemSize > this.binMaxSize)
                {
                    this.oversized.Add(item);
                }
                else
                {
                    int targetBin = -1;
                    ulong spaceRemain = this.binMaxSize + 1;

                    for (int b = 0; b < this.bins.Count; b++)
                    {
                        Bin currentBin = this.bins[b];
                        
                        // Prevent underflow by ensuring the item fits before subtraction
                        if (currentBin.size + item.itemSize <= this.binMaxSize)
                        {
                            ulong binSpaceLeft = this.binMaxSize - (currentBin.size + item.itemSize);

                            if (binSpaceLeft < spaceRemain)
                            {
                                spaceRemain = binSpaceLeft;
                                targetBin = b;
                            }
                        }
                    }
                    
                    if (targetBin >= 0)
                    {
                        this.bins[targetBin].Item_add(item.displayName, item.itemPath, item.itemSize);
                    }
                    else
                    {
                        Bin newbin = new Bin();
                        newbin.Item_add(item.displayName, item.itemPath, item.itemSize);
                        this.bins.Add(newbin);
                    }
                }
            }
            
            // Instantly clear the list allocation at the end to prevent O(N^2) memory shuffling
            this.list.Clear();
        }

        public void BestFitDesc()
        {
            this.listSortDesc();
            this.BestFit();
        }
    }
}
