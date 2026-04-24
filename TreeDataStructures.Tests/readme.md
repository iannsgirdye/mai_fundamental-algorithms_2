# Тестирование

## Сборка
``` bash
dotnet build
```

## Общий тест
``` bash
dotnet test
```

## Конкретный тест
**BinarySearchTree**
``` bash
dotnet test --filter "Category=BST"
```

**Treap**
``` bash
dotnet test --filter "Category=Treap"
```

**AVLTree**
``` bash
dotnet test --filter "Category=AVL"
```

**RedBlackTree**
``` bash
dotnet test --filter "Category=RB"
```

**SplayTree**
``` bash
dotnet test --filter "Category=Splay"
```
