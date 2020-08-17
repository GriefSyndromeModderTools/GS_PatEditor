using GS_PatEditor.Editor.Editable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Editor.Exporters.Enemy
{
    [Serializable]
    public class ActionListItemList : IEditableList<AbstractActionListItem>, IEnumerable<AbstractActionListItem>
    {
        [XmlElement(ElementName = "Item")]
        public List<AbstractActionListItem> Items = new List<AbstractActionListItem>();

        public int Count => Items.Count;

        public void Add(AbstractActionListItem val)
        {
            Items.Add(val);
        }

        public int FindIndex(AbstractActionListItem val)
        {
            return Items.FindIndex(i => i == val);
        }

        public IEnumerator<AbstractActionListItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public void Insert(int index, AbstractActionListItem val)
        {
            Items.Insert(index, val);
        }

        public void Remove(AbstractActionListItem val)
        {
            Items.Remove(val);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
