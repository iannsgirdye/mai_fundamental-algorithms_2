using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
{
    /// <summary>
    /// Разрезает дерево с корнем <paramref name="root"/> на два поддерева:
    /// Left: все ключи <= <paramref name="key"/>
    /// Right: все ключи > <paramref name="key"/>
    /// </summary>
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) Split(TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null)
        {
            return (null, null);
        }
        if (Comparer.Compare(key, root.Key) <= 0)
        {
            (var newLeft, var newRight) = Split(root.Left, key);
            root.Left = newRight;
            root.Left?.Parent = root;
            return (newLeft, root);
        }
        else
        {
            (var newLeft, var newRight) = Split(root.Right, key);
            root.Right = newLeft;
            root.Right?.Parent = root;
            return (root, newRight);
        }
    }

    /// <summary>
    /// Сливает два дерева в одно.
    /// Важное условие: все ключи в <paramref name="left"/> должны быть меньше ключей в <paramref name="right"/>.
    /// Слияние происходит на основе Priority (куча).
    /// </summary>
    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left == null || right == null)
        {
            return left ?? right;
        }
        if (left.Priority >= right.Priority)
        {
            left.Right = Merge(left.Right, right);
            left.Right?.Parent = left;
            return left;
        }
        else
        {
            right.Left = Merge(left, right.Left);
            right.Left?.Parent = right;
            return right;
        }
    }

    private (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) SplitLess(TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null)
        {
            return (null, null);
        }
        if (Comparer.Compare(key, root.Key) < 0)
        {
            (var left, var right) = SplitLess(root.Left, key);
            root.Left = right;
            root.Left?.Parent = root;
            return (left, root);
        }
        else
        {
            (var left, var right) = SplitLess(root.Right, key);
            root.Right = left;
            root.Right?.Parent = root;
            return (root, right);
        }
    }
    

    public override void Add(TKey key, TValue value)
    {
        var existingNode = FindNode(key);
        if (existingNode != null)
        {
            existingNode.Value = value;
            return;
        }

        var newNode = CreateNode(key, value);
        if (Root == null)
        {
            Root = newNode;
            return;
        }

        (var left, var right) = Split(Root, key);
        Root = Merge(Merge(left, newNode), right);
        Root?.Parent = null;
        
        this.Count++;
        OnNodeAdded(newNode);
    }

    public override bool Remove(TKey key)
    {
        if (Root == null || FindNode(key) == null)
        {
            return false;
        }

        (var left, var right) = Split(Root, key);
        (var lessNode, var deleteNode) = SplitLess(left, key);
        Root = Merge(lessNode, right);
        Root?.Parent = null;
        
        if (deleteNode != null)
        {
            this.Count--;
            OnNodeRemoved(null, null);
            return true;
        }
        return false;
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value) => new(key, value);
    
    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode) { }
    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child) { }
}