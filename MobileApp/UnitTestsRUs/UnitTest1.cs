using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestsRUs;

[TestFixture]
public class UnitTest1
{
    [Test]
    public void IsPrime_InputIs1_ReturnFalse()
    {
        var result = 1;

        Assert.That(result, Is.EqualTo(2));
    }
}
