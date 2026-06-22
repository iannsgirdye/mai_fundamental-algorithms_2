# ЛР по ФА на ФИИТе в МАИ
Лабораторные работы по дисциплине "Фундаментальные алгоритмы" в четвёртом семестре 2025/2026 учебного года направления "Фундаментальная информатика и информационные технологии" в [Московском авиационном институте](https://mai.ru)

- [Лабораторная работа №1](Tasks/lab1.md) — Ассоциативные структуры данных
- [Лабораторная работа №2](Tasks/lab2.md) — Длинные числа

## Сборка и запуск
### Переход в папку с тестами
``` bash
cd /<lab-name>.Tests/
```
`<lab-name>`: `TreeDataStructures` или `Arithmetic`

### Сборка
``` bash
dotnet build
```

### Тестирование
**Все категории**
``` bash
dotnet test
```

**Одна категория**
``` bash
dotnet test --filter "Category=<category-name>"
```

**Несколько категорий**
``` bash
dotnet test --filter "Category=<category-name>|Category=<category-name>"
```

| file name                | `<category-name>`         |
|--------------------------|---------------------------|
| `BinarySearchTree.cs`    | `BST`                     |
| `Treap.cs`               | `Treap`                   |
| `AvlTree.cs`             | `AVL`                     |
| `RedBlackTree.cs`        | `RB`                      |
| `SplayTree.cs`           | `Splay`                   |
| `BetterBigInteger.cs`    | `Base` & `Bitwise`        |
| `SimpleMultiplier.cs`    | `MultiplicationSimple`    |
| `KaratsubaMultiplier.cs` | `MultiplicationKaratsuba` |
| `FftMultiplier.cs`       | `MultiplicationFFT`       |
