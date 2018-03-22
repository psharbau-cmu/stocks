using System;
using System.Collections.Generic;
using System.Text;
using NeuralNet.Builder;
using NUnit.Framework;

namespace NeuralNet.Tests
{
    class TurZebGirTests
    {
        [Test]
        public void TurtleZebraGiraffe()
        {
            var tests = new[]
            {
                Tuple.Create(new[] {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f}, new[] {0f, 0f, 1f}),
                Tuple.Create(new[] {1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f}, new[] {1f, 0f, 0f}),
                Tuple.Create(new[] {1f, 1f, 1f, 1f, -1f, -1f, -1f, -1f, -1f, -1f}, new[] {0f, 1f, 0f})
            };
            var description = SimpleDescriptionBuilder.GetDescription(10, new[] {5, 5, 5, 3});


        }
    }
}
