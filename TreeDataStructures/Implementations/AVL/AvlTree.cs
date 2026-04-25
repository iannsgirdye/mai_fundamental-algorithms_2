using System;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    private int GetHeight(AvlNode<TKey, TValue>? node) => node?.Height ?? 0;

    private int UpdateHeight(AvlNode<TKey, TValue>? node) => Math.Max(GetHeight(node?.Left), GetHeight(node?.Right)) + 1;

    private int GetBalanceFactor(AvlNode<TKey, TValue>? node) => GetHeight(node?.Left) - GetHeight(node?.Right);

    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value) => new(key, value);

    private void Balance(AvlNode<TKey, TValue> newNode, bool isInsert)
    {
        var current = newNode.Parent;
        int balanceFactor = 0;
        int childBalanceFactor = 0;
        while (current != null)
        {
            UpdateHeight(current);
            balanceFactor = GetBalanceFactor(current);
            if (balanceFactor == 2)
            {
                childBalanceFactor = GetBalanceFactor(current.Left);
                if (childBalanceFactor == -1)
                {
                    RotateLeft(current.Left);
                    UpdateHeight(current.Left);
                    UpdateHeight(current.Left?.Parent);
                }
                RotateRight(current);
                UpdateHeight(current);
                UpdateHeight(current.Parent);

                if (isInsert) { return; }
            }
            else if (balanceFactor == -2)
            {
                childBalanceFactor = GetBalanceFactor(current.Right);
                if (childBalanceFactor == 1)
                {
                    RotateRight(current.Right);
                    UpdateHeight(current.Right);
                    UpdateHeight(current.Right?.Parent);
                }
                RotateLeft(current);
                UpdateHeight(current);
                UpdateHeight(current.Parent);

                if (isInsert) { return; }
            }

            current = current.Parent;
        }
    }

    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        throw new NotImplementedException();
}

    protected override void OnNodeRemoved(AvlNode<TKey, TValue>? parent, AvlNode<TKey, TValue>? child)
    {
        throw new NotImplementedException();
    }
}