using System.Collections;
using System.ComponentModel;
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

    public ICollection<TKey> Keys => throw new NotImplementedException();
    public ICollection<TValue> Values => throw new NotImplementedException();


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
        int cmp;
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
        if (x.Right != null) { x.Right.Parent = x; }

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
        if (y.Left != null) { y.Left.Parent = y; }

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
    
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrder() => InOrderTraversal(Root);
    
    private IEnumerable<TreeEntry<TKey, TValue>>  InOrderTraversal(TNode? node)
    {
        if (node == null) {  yield break; }
        throw new NotImplementedException();
    }
    
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrder() => throw new NotImplementedException();
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrder() => throw new NotImplementedException();
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrderReverse() => throw new NotImplementedException();
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrderReverse() => throw new NotImplementedException();
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrderReverse() => throw new NotImplementedException();
    
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
        private Stack<(TNode node, int depth)>? _stack;
        private TNode? _current;
        private int _currentDepth;
        private TNode? _prev;
        private bool _initialized;

        public TreeIterator(TNode? root, TraversalStrategy strategy)
        {
            _root = root;
            _strategy = strategy;
            _stack = new Stack<(TNode, int)>();
            _current = null;
            _currentDepth = -1;
            _prev = null;
            _initialized = false;
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
            if (!_initialized)
            {
                InitializeStack();
                _initialized = true;
            }

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

        public void Reset()
        {
            _stack?.Clear();
            _current = null;
            _currentDepth = -1;
            _prev = null;
            _initialized = false;
        }


        public void Dispose()
        {
            _stack = null;
        }

        private void InitializeStack()
        {
            if (_root == null) return;

            switch (_strategy)
            {
                case TraversalStrategy.InOrder:
                    PushLeftChain(_root, 0);
                    break;
                case TraversalStrategy.InOrderReverse:
                    PushRightChain(_root, 0);
                    break;
                case TraversalStrategy.PreOrder:
                    _stack!.Push((_root, 0));
                    break;
                case TraversalStrategy.PreOrderReverse:
                    _stack!.Push((_root, 0));
                    break;
                case TraversalStrategy.PostOrder:
                    _stack!.Push((_root, 0));
                    break;
                case TraversalStrategy.PostOrderReverse:
                    _stack!.Push((_root, 0));
                    break;
            }
        }
    }

    private void PushLeftChain(TNode? node, int depth)
    {
        while (node != null)
        {
            _stack!.Push((node, depth));
            node = node.Left;
            depth++;
        }
    }

    private void PushRightChain(TNode? node, int depth)
    {
        while (node != null)
        {
            _stack!.Push((node, depth));
            node = node.Right;
            depth++;
        }
    }

    private bool MoveNextInOrder()
    {
        if (_stack!.Count == 0) { return false };
        var (node, depth) = _stack.Pop();
        _current = node;
        _currentDepth = depth;
        PushLeftChain(node.Right, depth + 1);
        return true;
    }

    private bool MoveNextInOrderReverse()
    {
        if (_stack!.Count == 0) { return false };
        var (node, depth) = _stack.Pop();
        _current = node;
        _currentDepth = depth;
        PushRightChain(node.Left, depth + 1);
        return true;
    }

    private bool MoveNextPreOrder()
    {
        if (_stack!.Count == 0) { return false };
        var (node, depth) = _stack.Pop();
        _current = node;
        _currentDepth = depth;
        if (node.Right != null) _stack.Push((node.Right, depth + 1));
        if (node.Left != null) _stack.Push((node.Left, depth + 1));
        return true;
    }

    private bool MoveNextPreOrderReverse()
    {
        if (_stack!.Count == 0) return false;
        var (node, depth) = _stack.Pop();
        _current = node;
        _currentDepth = depth;
        if (node.Left != null) _stack.Push((node.Left, depth + 1));
        if (node.Right != null) _stack.Push((node.Right, depth + 1));
        return true;
    }

    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        throw new NotImplementedException();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}