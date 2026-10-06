using NUnit.Framework;
using UnityEngine;

public class InputTransformerTests
{
    [Test]
    public void NormalInputTest()
    {
        InputTransformer transformer = new NormalInputTransformer();

        Vector2 input = new Vector2(1, 0);

        Vector2 result = transformer.TransformMovement(input);

        Assert.AreEqual(new Vector2(1, 0), result);
    }

    [Test]
    public void InvertedInputTest()
    {
        InputTransformer transformer = new InvertedInputTransformer();

        Vector2 input = new Vector2(1, 0);

        Vector2 result = transformer.TransformMovement(input);

        Assert.AreEqual(new Vector2(-1, 0), result);
    }

    [Test]
    public void DynamicBindingTest()
    {
        InputTransformer transformer = new NormalInputTransformer();

        Vector2 input = new Vector2(1, 0);

        Vector2 result = transformer.TransformMovement(input);

        Assert.AreEqual(new Vector2(1, 0), result);

        transformer = new InvertedInputTransformer();

        result = transformer.TransformMovement(input);

        Assert.AreEqual(new Vector2(-1, 0), result);
    }
}