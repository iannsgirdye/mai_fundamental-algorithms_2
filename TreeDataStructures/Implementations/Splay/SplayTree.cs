using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);

    private void Splay(BstNode<TKey, TValue>? node)
    {
        while (node != null && node.Parent != null)
        {
            if (node.IsLeftChild && !node.hasGrandparent)
            {
                RotateRight(node.Parent);
            }
            else if (node.IsRightChild && !node.hasGrandparent)
            {
                RotateLeft(node.Parent);
            }
            else if (node.Parent.IsLeftChild && node.IsLeftChild)
            {
                RotateRight(node.Parent.Parent);
                RotateRight(node.Parent);
            }
            else if (node.Parent.IsLeftChild && node.IsRightChild)
            {
                RotateLeft(node.Parent);
                RotateRight(node.Parent);
            }
            else if (node.Parent.IsRightChild && node.IsRightChild)
            {
                RotateLeft(node.Parent.Parent);
                RotateLeft(node.Parent);
            }
            else if (node.Parent.IsRightChild && node.IsLeftChild)
            {
                RotateRight(node.Parent);
                RotateLeft(node.Parent);
            }
            node = node.Parent?.Parent;
        }
    }

    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        Splay(newNode);
    }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child) { }
    
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindNode(key);
        value = (node == null) ? default : node.Value;
        if (node != null) { Splay(node); }
        return node != null;
    }

    public override bool ContainsKey(TKey key)
    {
        var node = FindNode(key);
        if (node != null)
        {
            Splay(node);
        }
        return node != null;
    }
}
