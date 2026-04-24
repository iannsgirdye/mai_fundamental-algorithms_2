using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null)
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default; // use it to compare Keys

    public int Count { get; protected set; }

    public bool IsReadOnly => false;

    public ICollection<TKey> Keys
    {
        get
        {
            var keys = new List<TKey>();
            foreach (var entry in InOrder())
            {
                keys.Add(entry.Key);
            }
            return keys;
        }
    }
    public ICollection<TValue> Values
    {
        get
        {
            var values = new List<TValue>();
            foreach (var entry in InOrder())
            {
                values.Add(entry.Value);
            }
            return values;
        }
    }


    public virtual void Add(TKey key, TValue value)
    {
        TNode newNode = CreateNode(key, value);
        if (Root == null)
        {
            Root = newNode;
            Count++;
            OnNodeAdded(newNode);
            return;
        }

        TNode current = Root;
        TNode? parent = null;
        int cmp = 0;
        while (current != null)
        {
            cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0)
            {
                current.Value = value;
                return;
            }
            parent = current;
            current = cmp < 0 ? current.Left : current.Right;
        }

        if (cmp < 0)
        {
            parent.Left = newNode;
        }
        else
        {
            parent.Right = newNode;
        }
        newNode.Parent = parent;
        Count++;
        OnNodeAdded(newNode);
    }


    public virtual bool Remove(TKey key)
    {
        TNode? node = FindNode(key);
        if (node == null) { return false; }

        RemoveNode(node);
        this.Count--;
        return true;
    }


    protected virtual void RemoveNode(TNode node)
    {
        TNode deleteNode = node;
        TNode? parent = deleteNode.Parent;
        TNode? replacement;

        if (deleteNode.Left != null && deleteNode.Right != null)
        {
            deleteNode = FindMinimum(deleteNode.Right)!;
            parent = deleteNode.Parent;
            node.Key = deleteNode.Key;
            node.Value = deleteNode.Value;
        }

        replacement = deleteNode.Left ?? deleteNode.Right;
        Transplant(deleteNode, replacement);
        OnNodeRemoved(parent, replacement);
    }

    public virtual bool ContainsKey(TKey key) => FindNode(key) != null;

    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        TNode? node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out TValue? val) ? val : throw new KeyNotFoundException();
        set => Add(key, value);
    }


    #region Hooks

    /// <summary>
    /// Вызывается после успешной вставки
    /// </summary>
    /// <param name="newNode">Узел, который встал на место</param>
    protected virtual void OnNodeAdded(TNode newNode) { }

    /// <summary>
    /// Вызывается после удаления. 
    /// </summary>
    /// <param name="parent">Узел, чей ребенок изменился</param>
    /// <param name="child">Узел, который встал на место удаленного</param>
    protected virtual void OnNodeRemoved(TNode? parent, TNode? child) { }

    #endregion


    #region Helpers
    protected abstract TNode CreateNode(TKey key, TValue value);


    protected TNode? FindNode(TKey key)
    {
        TNode? current = Root;
        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) { return current; }
            current = cmp < 0 ? current.Left : current.Right;
        }
        return null;
    }

    private TNode? FindMinimum(TNode node)
    {
        if (node == null) { return null; }
        while (node.Left != null) { node = node.Left; }
        return node;
    }

    protected void RotateLeft(TNode x)
    {
        if (x == null || x.Right == null) { return; }
        TNode child = x.Right;
        TNode parent = x.Parent;

        x.Right = child.Left;
        x.Right?.Parent = x;

        child.Left = x;
        x.Parent = child;

        if (parent == null) { Root = child; }
        else {
            if (x.IsLeftChild) { parent.Left = child; }
            else { parent.Right = child; }
        }
        child.Parent = parent;
    }

    protected void RotateRight(TNode y)
    {
        if (y == null || y.Left == null) { return; }
        TNode child = y.Left;
        TNode parent = y.Parent;

        y.Left = child.Right;
        y.Left?.Parent = y;

        child.Right = y;
        y.Parent = child;

        if (parent == null) { Root = child; }
        else {
            if (y.IsLeftChild) { parent.Left = child; }
            else { parent.Right = child; }
        }
        child.Parent = parent;
    }

    protected void RotateBigLeft(TNode x)
    {
        RotateRight(x);
        RotateLeft(x);
    }
    
    protected void RotateBigRight(TNode y)
    {
        RotateLeft(y);
        RotateRight(y);
    }
    
    protected void RotateDoubleLeft(TNode x)
    {
        RotateLeft(x);
        RotateLeft(x);
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        RotateRight(y);
        RotateRight(y);
    }
    
    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
        {
            Root = v;
        }
        else if (u.IsLeftChild)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }
        v?.Parent = u.Parent;
    }
    #endregion

    public IEnumerable<TreeEntry<TKey, TValue>> InOrder() => new TreeIterator(Root, TraversalStrategy.InOrder);
    public IEnumerable<TreeEntry<TKey, TValue>> PreOrder() => new TreeIterator(Root, TraversalStrategy.PreOrder);
    public IEnumerable<TreeEntry<TKey, TValue>> PostOrder() => new TreeIterator(Root, TraversalStrategy.PostOrder);
    public IEnumerable<TreeEntry<TKey, TValue>> InOrderReverse() => new TreeIterator(Root, TraversalStrategy.InOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>> PreOrderReverse() => new TreeIterator(Root, TraversalStrategy.PreOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>> PostOrderReverse() => new TreeIterator(Root, TraversalStrategy.PostOrderReverse);

    /// <summary>
    /// Внутренний класс-итератор. 
    /// Реализует паттерн Iterator вручную, без yield return (ban).
    /// </summary>
    private struct TreeIterator :
        IEnumerable<TreeEntry<TKey, TValue>>,
        IEnumerator<TreeEntry<TKey, TValue>>
    {
        private readonly TNode? _root;
        private readonly TraversalStrategy _strategy;
        private TNode? _current;
        private int _currentDepth;
        private bool _started;

        public TreeIterator(TNode? root, TraversalStrategy strategy)
        {
            _root = root;
            _strategy = strategy;
            Reset();
        }

        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;

        public TreeEntry<TKey, TValue> Current
        {
            get
            {
                if (_current == null)
                    throw new InvalidOperationException("Iterator not moved to an element.");
                return new TreeEntry<TKey, TValue>(_current.Key, _current.Value, _currentDepth);
            }
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (!_started)
            {
                if (_root == null) { return false; }
                InitStart();
                _started = true;
                return true;
            }
            else
            {
                switch (_strategy)
                {
                    case TraversalStrategy.InOrder: return MoveNextInOrder();
                    case TraversalStrategy.PreOrder: return MoveNextPreOrder();
                    case TraversalStrategy.PostOrder: return MoveNextPostOrder();
                    case TraversalStrategy.InOrderReverse: return MoveNextInOrderReverse();
                    case TraversalStrategy.PreOrderReverse: return MoveNextPreOrderReverse();
                    case TraversalStrategy.PostOrderReverse: return MoveNextPostOrderReverse();
                    default: return false;
                }
            }
        }

        public void Reset() {
            _current = null;
            _currentDepth = -1;
            _started = false;
        }

        public void Dispose() { }

        private void InitStart()
        {
            _current = _root;
            _currentDepth = 0;
            switch (_strategy)
            {
                case TraversalStrategy.InOrder: return InitStartInOrder();
                case TraversalStrategy.InOrderReverse: return InitStartInOrderReverse();
                case TraversalStrategy.PreOrder: return;
                case TraversalStrategy.PreOrderReverse: return;
                case TraversalStrategy.PostOrder: return InitStartPostOrder();
                case TraversalStrategy.PostOrderReverse: return InitStartPostOrderReverse();
            }
        }

        private void InitStartInOrder()
        {
            while (_current!.Left != null)
            {
                _current = _current.Left;
                _currentDepth++;
            }
        }

        private void InitStartInOrderReverse()
        {
            while (_current!.Right != null)
            {
                _current = _current.Right;
                _currentDepth++;
            }
        }

        private void InitStartPostOrder()
        {
            while (_current!.Left != null || _current!.Right != null)
            {
                if (_current!.Left != null) { _current = _current.Left; }
                else { _current = _current.Right; }
                _currentDepth++;
            }
        }

        private void InitStartPostOrderReverse()
        {
            while (_current!.Left != null || _current!.Right != null)
            {
                if (_current!.Right != null) { _current = _current.Right; }
                else { _current = _current.Left; }
                _currentDepth++;
            }
        }

        private bool MoveNextInOrder()
        {
            if (_current == null)
            {
                return false;
            }
            if (_current.Right != null)
            {
                _current = _current.Right;
                _currentDepth++;
                while (_current.Left != null)
                {
                    _current = _current.Left;
                    _currentDepth++;
                }
                return true;
            }

            TNode? parent = _current.Parent;
            while (parent != null && parent.Right == _current)
            {
                _current = parent;
                parent = parent.Parent;
                _currentDepth--;
            }
            if (parent != null)
            {
                _current = parent;
                _currentDepth--;
                return true;
            }
            _current = null;
            return false;
        }

        private bool MoveNextInOrderReverse()
        {
            if (_current == null)
            {
                return false;
            }
            if (_current.Left != null)
            {
                _current = _current.Left;
                _currentDepth++;
                while (_current.Right != null)
                {
                    _current = _current.Right;
                    _currentDepth++;
                }
                return true;
            }

            TNode? parent = _current.Parent;
            while (parent != null && parent.Left == _current)
            {
                _current = parent;
                parent = parent.Parent;
                _currentDepth--;
            }
            if (parent != null)
            {
                _current = parent;
                _currentDepth--;
                return true;
            }
            _current = null;
            return false;
        }

        private bool MoveNextPreOrder()
        {
            if (_current == null)
            {
                return false;
            }
            if (_current.Left != null)
            {
                _current = _current.Left;
                _currentDepth++;
                return true;
            }
            if (_current.Right != null)
            {
                _current = _current.Right;
                _currentDepth++;
                return true;
            }
            TNode? parent = _current.Parent;

            while (parent != null && (parent.Right == null || parent.Right == _current))
            {
                _current = parent;
                parent = parent.Parent;
                _currentDepth--;
            }
            if (parent != null)
            {
                _current = parent.Right;
                return true;
            }
            else
            {
                _current = null;
                return false;
            }
        }

        private bool MoveNextPreOrderReverse()
        {
            if (_current == null)
            {
                return false;
            }
            if (_current.Right != null)
            {
                _current = _current.Right;
                _currentDepth++;
                return true;
            }
            if (_current.Left != null)
            {
                _current = _current.Left;
                _currentDepth++;
                return true;
            }
            TNode? parent = _current.Parent;

            while (parent != null && (parent.Left == null || parent.Left == _current))
            {
                _current = parent;
                parent = parent.Parent;
                _currentDepth--;
            }
            if (parent != null)
            {
                _current = parent.Left;
                return true;
            }
            else
            {
                _current = null;
                return false;
            }
        }

        private bool MoveNextPostOrder()
        {
            if (_current == null)
            {
                return false;
            }
            TNode parent = _current.Parent;
            if (parent != null)
            {
                if (parent.Left == _current)
                {
                    _current = parent;
                    _currentDepth--;
                    if (_current.Right != null)
                    {
                        _current = _current.Right;
                        _currentDepth++;
                        while (_current.Left != null)
                        {
                            _current = _current.Left;
                            _currentDepth++;
                        }
                    }
                }
                else
                {
                    _current = parent;
                    _currentDepth--;
                }
                return true;
            }
            return false;
        }

        private bool MoveNextPostOrderReverse()
        {
            if (_current == null)
            {
                return false;
            }
            TNode parent = _current.Parent;
            if (parent != null)
            {
                if (parent.Right == _current)
                {
                    _current = parent;
                    _currentDepth--;
                    if (_current.Left != null)
                    {
                        _current = _current.Left;
                        _currentDepth++;
                        while (_current.Right != null)
                        {
                            _current = _current.Right;
                            _currentDepth++;
                        }
                    }
                }
                else
                {
                    _current = parent;
                    _currentDepth--;
                }
                return true;
            }
            return false;
        }
    }

    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var entry in InOrder())
            yield return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        foreach (var entry in InOrder())
        {
            if (arrayIndex >= array.Length)
                throw new ArgumentException("Array too small");
            array[arrayIndex++] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}