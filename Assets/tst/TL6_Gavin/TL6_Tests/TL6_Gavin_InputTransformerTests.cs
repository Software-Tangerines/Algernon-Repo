
using NUnit.Framework;
using UnityEngine;

public class InputTransformerTests
{
    [TestCase(1, 0)]
    [TestCase(-1, 0)]
    [TestCase(0, 1)]
    [TestCase(0, -1)]
    public void NormalInputTest(float x, float y)
    {
        InputTransformer transformer = new NormalInputTransformer();

        Vector2 input = new Vector2(x, y);
        Vector2 result = transformer.TransformMovement(input);

        Assert.AreEqual(input, result);
    }

    [TestCase(1, 0)]
    [TestCase(-1, 0)]
    [TestCase(0, 1)]
    [TestCase(0, -1)]
    public void InvertedInputTest(float x, float y)
    {
        InputTransformer transformer = new InvertedInputTransformer();

        Vector2 input = new Vector2(x, y);
        Vector2 result = transformer.TransformMovement(input);

        Assert.AreEqual(-input, result);
    }

    [Test]
    public void DynamicBindingChangesTransformer()
    {
        InputTransformer transformer = new NormalInputTransformer();

        Vector2 input = Vector2.right;

        Assert.AreEqual(Vector2.right,
            transformer.TransformMovement(input));

        transformer = new InvertedInputTransformer();

        Assert.AreEqual(Vector2.left,
            transformer.TransformMovement(input));
    }

    [Test]
    public void NormalInputDoesNotChangeZero()
    {
        InputTransformer transformer = new NormalInputTransformer();

        Assert.AreEqual(Vector2.zero,
            transformer.TransformMovement(Vector2.zero));
    }

    [Test]
    public void InvertedInputDoesNotChangeZero()
    {
        InputTransformer transformer = new InvertedInputTransformer();

        Assert.AreEqual(Vector2.zero,
            transformer.TransformMovement(Vector2.zero));
    }
}
