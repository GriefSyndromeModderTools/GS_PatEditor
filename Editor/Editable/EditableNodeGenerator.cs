using GS_PatEditor.Localization;
using GS_PatEditor.Localization.Elements;
using GS_PatEditor.Pat.Effects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor.Editable
{
    class EditableNodeGenerator
    {
        private class GenericEditableTreeNode<T> : EditableTreeNode<T>
            where T : class
        {
            public GenericEditableTreeNode(EditableEnvironment env, object data, Editable<T> dest, string title)
                : base(env, data as T, data, dest, title)
            {
                if (data is IEditableEnvironment)
                {
                    var cv = (IEditableEnvironment)data;
                    if (cv.Environment == null)
                    {
                        cv.Environment = env;
                    }
                }
            }

            public GenericEditableTreeNode(EditableEnvironment env, MultiEditable<T> dest, string title)
                : base(env, dest, title)
            {
            }

            protected override object CreateSelectObject(MultiEditable<T> dest)
            {
                return new SelectObject<T>(obj =>
                {
                    dest.Append((T)obj);
                    this.InsertBefore(new GenericEditableTreeNode<T>(Env, obj, dest, Title));
                }, Env);
            }

            protected override TreeNode CreateSingleEditableNode(SingleEditable<T> dest)
            {
                GenericEditableTreeNode<T> ret = null;
                ret = new GenericEditableTreeNode<T>(Env, new SelectObject<T>(obj =>
                {
                    dest.Reset((T)obj);
                    ret.Replace(new GenericEditableTreeNode<T>(Env, obj, dest, Title));
                }, Env), null, Title);
                return ret;
            }

            private string MakeText(string str)
            {
                if (Title == null)
                {
                    return str;
                }
                return String.Format(EditableTreeViewRes.NameAndTextFormat, Title, str);
            }

            protected override void SetupCommon()
            {
                if (Data == null)
                {
                    Text = MakeText(EditableTreeViewRes.Select);
                }
                else if (Data is IDataNodeDisplayNameProvider)
                {
                    var cdata = (IDataNodeDisplayNameProvider)Data;
                    if (cdata.OverrideFullName)
                    {
                        Text = cdata.DisplayName;
                    }
                    else
                    {
                        Text = MakeText(cdata.DisplayName);
                    }
                }
                else
                {
                    var type = Data.GetType();
                    var dn = type.GetCustomAttribute<DisplayNameAttribute>();
                    if (dn != null)
                    {
                        Text = MakeText(dn.DisplayName);
                    }
                    else
                    {
                        Text = MakeText(type.Name);
                    }
                }

                EditableNodeGenerator.SetupChildren<T>(this, Env, Data);
            }
        }

        private class FieldSingleEditable<T> : SingleEditable<T>
            where T : class
        {
            private object _Object;
            private FieldInfo _Field;

            public FieldSingleEditable(object obj, FieldInfo field)
            {
                _Object = obj;
                _Field = field;
            }

            public void Reset(T value)
            {
                _Field.SetValue(_Object, value);
            }
        }

        public static EditableTreeNode<T> Create<T>(EditableEnvironment env, T value, Editable<T> dest, string title = null)
            where T : class
        {
            return new GenericEditableTreeNode<T>(env, value, dest, title);
        }

        public static EditableTreeNode<T> Create<T>(EditableEnvironment env, MultiEditable<T> dest, string title = null)
            where T : class
        {
            return new GenericEditableTreeNode<T>(env, dest, title);
        }

        private static void SetupChildren<T>(TreeNode node, EditableEnvironment env, T obj)
        {
            if (obj == null)
            {
                return;
            }

            var fields = obj.GetType().GetFields();
            foreach (var f in fields)
            {
                var attr = f.GetCustomAttributes(typeof(EditorChildNodeAttribute), false)
                    .Select(x => (EditorChildNodeAttribute)x)
                    .FirstOrDefault();
                if (attr == null)
                {
                    continue;
                }

                TreeNodeCollection coll;
                if (attr.Name == null || attr.Merged)
                {
                    coll = node.Nodes;
                }
                else
                {
                    var newNode = new TreeNode { Text = attr.Name };
                    newNode.Expand();
                    node.Nodes.Add(newNode);
                    coll = newNode.Nodes;
                }

                var valueF = f.GetValue(obj);
                var typeV = f.FieldType;
                var listInterface = typeV.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                        (i.GetGenericTypeDefinition() == typeof(IEditableList<>) ||
                         i.GetGenericTypeDefinition() == typeof(IList<>)))
                    .Select(i => new { Arg = i.GetGenericArguments()[0], Temp = i.GetGenericTypeDefinition() })
                    .FirstOrDefault();

                var typeString = typeof(string);
                var title = attr.Merged ? attr.Name : null;

                if (listInterface != null)
                {
                    //we've got a list
                    //first create our MultiEditable object
                    var editableT = typeof(Editable<>).MakeGenericType(listInterface.Arg);
                    var treeNodeT = typeof(GenericEditableTreeNode<>).MakeGenericType(listInterface.Arg);

                    object me;
                    if (listInterface.Temp == typeof(IEditableList<>))
                    {
                        var editableListT = typeof(IEditableList<>).MakeGenericType(listInterface.Arg);
                        me = typeof(EditableListMultiEditable<>).MakeGenericType(listInterface.Arg)
                            .GetConstructor(new Type[] { editableListT })
                            .Invoke(new object[] { valueF });
                    }
                    else
                    {
                        var editableListT = typeof(IList<>).MakeGenericType(listInterface.Arg);
                        me = typeof(ListMultiEditable<>).MakeGenericType(listInterface.Arg)
                            .GetConstructor(new Type[] { editableListT })
                            .Invoke(new object[] { valueF });
                    }
                    var creator1 = treeNodeT.GetConstructor(new Type[] {
                        typeof(EditableEnvironment), listInterface.Arg, editableT, typeString });
                    var creator2 = treeNodeT.GetConstructor(new Type[] {
                        typeof(EditableEnvironment),
                        typeof(MultiEditable<>).MakeGenericType(listInterface.Arg), typeString });

                    var list = (System.Collections.IEnumerable)valueF;
                    foreach (var item in list)
                    {
                        var newNode = (TreeNode)creator1.Invoke(new object[] { env, item, me, title });
                        coll.Add(newNode);
                    }
                    coll.Add((TreeNode)creator2.Invoke(new object[] { env, me, title }));
                }
                else
                {
                    var editableT = typeof(Editable<>).MakeGenericType(typeV);
                    var se = typeof(FieldSingleEditable<>).MakeGenericType(typeV)
                        .GetConstructor(new Type[] { typeof(object), typeof(FieldInfo) })
                        .Invoke(new object[] { obj, f });
                    var creator = typeof(GenericEditableTreeNode<>).MakeGenericType(typeV)
                        .GetConstructor(new Type[] { typeof(EditableEnvironment), typeV, editableT, typeString });
                    var newNode = (IEditableTreeNode)creator.Invoke(new object[] { env, valueF, se, title });
                    coll.Add((TreeNode)newNode);
                    if (valueF == null)
                    {
                        newNode.Reset();
                    }
                }
            }
        }
    }

    public interface IEditableList<T> : IEnumerable<T>
        where T : class
    {
        void Add(T val);
        void Remove(T val);
        int FindIndex(T val);
        void Insert(int index, T val);
        int Count { get; }
    }

    public class EditorChildNodeAttribute : Attribute
    {
        public string LocalizationID { get; private set; }
        public string Name { get; private set; }
        public bool Merged { get; private set; }

        public EditorChildNodeAttribute(string localizationID, bool merged = true)
        {
            LocalizationID = localizationID;
            Merged = merged;
            Name = Get();
        }

        private string Get()
        {
            if (LocalizationID == null)
            {
                return null;
            }
            try
            {
                return ChildNodeNameRes.ResourceManager.GetString(LocalizationID);
            }
            catch
            {
                return LocalizationID.Substring(LocalizationID.IndexOf('_') + 1);
            }
        }
    }
}
