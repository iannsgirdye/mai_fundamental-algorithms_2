using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
{
    private bool IsBlack(RbNode<TKey, TValue>? node) => node == null || node.Color == RbColor.Black;
    
    private bool IsRed(RbNode<TKey, TValue>? node) => !IsBlack(node);

    private void SetBlack(RbNode<TKey, TValue>? node)
    {
        node?.Color = RbColor.Black;
    }

    private void SetRed(RbNode<TKey, TValue>? node)
    {
        node?.Color = RbColor.Red;
    }

    private void SetColorFrom(RbNode<TKey, TValue>? source, RbNode<TKey, TValue>? target)
    {
        if (target != null)
        {
            source?.Color = target.Color;
        }
    }

    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value) => new(key, value);
    
    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        var node = newNode;
        while (node != null)
        {
            if (node.Parent == null)
            { 
                AddCase1(node);
                break;
            }
            else if (IsBlack(node.Parent))
            { 
                AddCase2(node);
                break;         
            }
            else if (IsRed(node.Uncle))  // Parent is Red;
            { 
                AddCase3(node);
                node = node.Grandparent;
            }
            else  // Parent is Red, Uncle is Black
            {
                AddCases45(node);
                break;
            }
        }
        if (IsRed(this.Root)) SetBlack(this.Root);
    }

    private void AddCase1(RbNode<TKey, TValue> node)
    {
        SetBlack(node);
    }

    private void AddCase2(RbNode<TKey, TValue> node) { }

    private void AddCase3(RbNode<TKey, TValue> node)
    {
        SetBlack(node.Parent);
        SetBlack(node.Uncle);
        SetRed(node.Grandparent);
    }

    private void AddCases45(RbNode<TKey, TValue> node)
    {
        if (node.Parent!.IsLeftChild)
        {
            if (node.IsRightChild)
            {
                RotateLeft(node.Parent);
                RotateRight(node.Parent);
                SetBlack(node);
                SetRed(node.Right);
            }
            else
            {
                RotateRight(node.Grandparent!);
                SetBlack(node.Parent);
                SetRed(node.Parent.Right);
            }
        }
        else
        {
            if (node.IsLeftChild)
            {
                RotateRight(node.Parent);
                RotateLeft(node.Parent);
                SetBlack(node);
                SetRed(node.Left);
            }
            else
            {
                RotateLeft(node.Grandparent!);
                SetBlack(node.Parent);
                SetRed(node.Parent.Left);
            }
        }
    }

    private RbColor _lastDeleteNodeColor;

    protected override void RemoveNode(RbNode<TKey, TValue> node) {
        RbNode<TKey, TValue> deleteNode = node;
        RbNode<TKey, TValue>? parent = deleteNode.Parent;
        RbNode<TKey, TValue>? replacement;

        if (deleteNode.Left != null && deleteNode.Right != null) {
            deleteNode = FindMinimum(deleteNode.Right)!;
            parent = deleteNode.Parent;
            node.Key = deleteNode.Key;
            node.Value = deleteNode.Value;
        }

        replacement = deleteNode.Left ?? deleteNode.Right;
        _lastDeleteNodeColor = deleteNode.Color;
        Transplant(deleteNode, replacement);
        OnNodeRemoved(parent, replacement);
    }

    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        throw new NotImplementedException();
    }
}