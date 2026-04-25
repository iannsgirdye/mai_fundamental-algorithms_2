using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
{
    private bool IsBlack(RbNode<TKey, TValue>? node) => node == null || node.Color == RbColor.Black;
    private bool IsRed(RbNode<TKey, TValue>? node) => !IsBlack(node);

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
        if (IsRed(this.Root)) this.Root?.Color = RbColor.Black;
    }

    private void AddCase1(RbNode<TKey, TValue> node)
    {
        node.Color = RbColor.Black;
    }

    private void AddCase2(RbNode<TKey, TValue> node) { }

    private void AddCase3(RbNode<TKey, TValue> node)
    {
        node.Parent!.Color = RbColor.Black;
        node.Uncle?.Color = RbColor.Black;
        node.Grandparent?.Color = RbColor.Red;
    }

    private void AddCases45(RbNode<TKey, TValue> node)
    {
        if (node.Parent!.IsLeftChild)
        {
            if (node.IsRightChild)
            {
                RotateLeft(node.Parent);
                RotateRight(node.Parent);
                node.Color = RbColor.Black;
                node.Right!.Color = RbColor.Red;
            }
            else
            {
                RotateRight(node.Grandparent!);
                node.Parent.Color = RbColor.Black;
                node.Parent.Right!.Color = RbColor.Red;
            }
        }
        else
        {
            if (node.IsLeftChild)
            {
                RotateRight(node.Parent);
                RotateLeft(node.Parent);
                node.Color = RbColor.Black;
                node.Left!.Color = RbColor.Red;
            }
            else
            {
                RotateLeft(node.Grandparent!);
                node.Parent.Color = RbColor.Black;
                node.Parent.Left!.Color = RbColor.Red;
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