using System;
using Friday.Common.Entities;
using NUnit.Framework;

namespace Friday.Tests;

public class HumanTimeTests
{
    [Test]
    public void Humanize_1()
    {
        var humanTimeSpan = new HumanTimeSpan(TimeSpan.FromMinutes(5));

        var humanized = humanTimeSpan.Humanize();
        
        Assert.AreEqual("5 minutes", humanized);
    }
    
    [Test]
    public void Humanize_2()
    {
        var humanTimeSpan = new HumanTimeSpan(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(30));

        var humanized = humanTimeSpan.Humanize();
        
        Assert.AreEqual("5 minutes 30 seconds", humanized);
    }
    
    [Test]
    public void Humanize_3()
    {
        var humanTimeSpan = new HumanTimeSpan(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(1));

        var humanized = humanTimeSpan.Humanize();
        
        Assert.AreEqual("5 minutes 1 second", humanized);
    }
    
    [Test]
    public void Humanize_4()
    {
        var humanTimeSpan = new HumanTimeSpan(TimeSpan.FromMinutes(1));

        var humanized = humanTimeSpan.Humanize();
        
        Assert.AreEqual("1 minute", humanized);
    }

    [Test]
    public void Parse_1()
    {
        var humanTimeSpan = HumanTimeSpan.Parse("5m");
        
        Assert.AreEqual(TimeSpan.FromMinutes(5), humanTimeSpan.Value);
    }
    
    [Test]
    public void Parse_2()
    {
        var humanTimeSpan = HumanTimeSpan.Parse("5m30s");
        
        Assert.AreEqual(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(30), humanTimeSpan.Value);
    }
    
    [Test]
    public void Parse_3()
    {
        var humanTimeSpan = HumanTimeSpan.Parse("5m1s");
        
        Assert.AreEqual(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(1), humanTimeSpan.Value);
    }
    
    [Test]
    public void Parse_4()
    {
        var humanTimeSpan = HumanTimeSpan.Parse("1d1h1m1s");
        
        Assert.AreEqual(TimeSpan.FromDays(1) + TimeSpan.FromHours(1) + TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(1), humanTimeSpan.Value);
    }

    [Test]
    public void ToHumanTime_1()
    {
        var humanTimeSpan = new HumanTimeSpan(TimeSpan.FromMinutes(5));
        
        var humanTime = humanTimeSpan.ToHumanTime();
        
        Assert.AreEqual("5m", humanTime);
    }
    
    [Test]
    public void ToHumanTime_2()
    {
        var humanTimeSpan = new HumanTimeSpan(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(30));
        
        var humanTime = humanTimeSpan.ToHumanTime();
        
        Assert.AreEqual("5m30s", humanTime);
    }
    
    [Test]
    public void ToHumanTime_3()
    {
        var humanTimeSpan = new HumanTimeSpan(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(1));
        
        var humanTime = humanTimeSpan.ToHumanTime();
        
        Assert.AreEqual("5m1s", humanTime);
    }
    
    [Test]
    public void ToHumanTime_4()
    {
        var humanTimeSpan = new HumanTimeSpan(TimeSpan.FromDays(1) + TimeSpan.FromHours(1) + TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(1));
        
        var humanTime = humanTimeSpan.ToHumanTime();
        
        Assert.AreEqual("1d1h1m1s", humanTime);
    }

    [Test]
    public void FiledParse_1()
    {
        try
        {
            HumanTimeSpan.Parse("5m3h");
            Assert.Fail();
        }catch
        {
            Assert.Pass();
        }
    }
    
    [Test]
    public void FiledParse_2()
    {
        try
        {
            HumanTimeSpan.Parse("5m3h1s");
            Assert.Fail();
        }catch
        {
            Assert.Pass();
        }
    }
    
    [Test]
    public void FiledParse_3()
    {
        try
        {
            HumanTimeSpan.Parse("5h3m1h");
            Assert.Fail();
        }catch
        {
            Assert.Pass();
        }
    }
    
}