using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);

    private void Splay(BstNode<TKey, TValue> node)
    {
        while (node.Parent != null)
        {
            if (node.Parent.IsLeftChild)
            {
                if (node.IsLeftChild)
                {
                    RotateRight(node.Parent.Parent);
                    RotateRight(node.Parent);
                }
                else
                {
                    RotateLeft(node.Parent);
                    RotateRight(node.Parent);
                }
                node = node.Parent.Parent;
            }
            else if (node.Parent.IsRightChild)
            {
                if (node.IsRightChild)
                {
                    RotateLeft(node.Parent.Parent);
                    RotateLeft(node.Parent);
                }
                else
                {
                    RotateRight(node.Parent);
                    RotateLeft(node.Parent);
                }
                node = node.Parent.Parent;
            }
            else  // node.Parent.Parent == null;
            {
                if (node.IsLeftChild)
                {
                    RotateRight(node.Parent);
                }
                else
                {
                    RotateLeft(node.Parent);
                }
                node = node.Parent;
            }
        }
    }

    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        throw new NotImplementedException();
    }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
        throw new NotImplementedException();
    }
    
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        throw new NotImplementedException();
    }
    
}
