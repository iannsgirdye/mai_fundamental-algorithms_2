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

    private void Balance(AvlNode<TKey, TValue> node, bool isAdd)
    {
        int balanceFactor = 0;
        int childBalanceFactor = 0;
        while (node != null)
        {
            UpdateHeight(node);
            balanceFactor = GetBalanceFactor(node);
            if (balanceFactor == 2)
            {
                childBalanceFactor = GetBalanceFactor(node.Left);
                if (childBalanceFactor == -1)
                {
                    RotateLeft(node.Left);
                    UpdateHeight(node.Left.Left);
                    UpdateHeight(node.Left);
                }
                RotateRight(node);
                UpdateHeight(node);
                UpdateHeight(node.Parent);

                if (isAdd) { return; }
            }
            else if (balanceFactor == -2)
            {
                childBalanceFactor = GetBalanceFactor(node.Right);
                if (childBalanceFactor == 1)
                {
                    RotateRight(node.Right);
                    UpdateHeight(node.Right.Right);
                    UpdateHeight(node.Right);
                }
                RotateLeft(node);
                UpdateHeight(node);
                UpdateHeight(node.Parent);

                if (isAdd) { return; }
            }

            node = node.Parent;
        }
    }

    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode) { Balance(newNode, isAdd: true); }

    protected override void OnNodeRemoved(AvlNode<TKey, TValue>? parent, AvlNode<TKey, TValue>? child) { Balance(parent, isAdd: false); }
}