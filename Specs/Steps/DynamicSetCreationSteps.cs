﻿using System;
using System.Linq;
using NUnit.Framework;
using Should.Fluent;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using SpecFlow.Assist.Dynamic;

namespace Specs.Steps
{
    [Binding]
    public class DynamicSetCreationSteps
    {
        private static dynamic GetItem(int itemNumber)
        {
            return State.OriginalSet[itemNumber - 1];
        }

        [Given(@"I create a set of dynamic instances from this table")]
        [When(@"I create a set of dynamic instances from this table")]
        public void WithMethodBInding(Table table)
        {
            State.OriginalSet = table.CreateDynamicSet().ToList();
        }

        [Given(@"I create a set of dynamic instances from this table using no type conversion")]
        public void WithMethodBIndingNoTypeConversion(Table table)
        {
            State.OriginalSet = table.CreateDynamicSet(new Options { DoTypeConversion = false }).ToList();
        }

        [Then(@"I should have a list of (\d+) dynamic objects")]
        public void ShouldContain(int expectedNumberOfItems)
        {
            State.OriginalSet.Count.Should().Equal(expectedNumberOfItems);
        }

        [Then(@"the (\d+) item should have BirthDate equal to '(.*)'")]
        public void ItemInSetShouldHaveExpectedBirthDate(int itemNumber, string expectedBirthDate)
        {
            Assert.AreEqual(DateTime.Parse(expectedBirthDate), GetItem(itemNumber).BirthDate);
        }

        [Then(@"the (\d+) item should have Age equal to '(\d+)'")]
        public void ItemInSetShouldHaveExpectedAge(int itemNumber, int expectedAge)
        {
            Assert.AreEqual(expectedAge, GetItem(itemNumber).Age);
        }

        [Then(@"the (.*) item should still Name equal '(.*)'")]
        public void ThenTheItemShouldStillNameEqual(int itemNumber, string expectedName)
        {
            Assert.AreEqual(expectedName, GetItem(itemNumber).Name);
        }

        [Then(@"the (.*) item should still Age equal '(.*)'")]
        public void ThenTheItemShouldStillAgeEqual(int itemNumber, string expectedAge)
        {
            Assert.AreEqual(expectedAge, GetItem(itemNumber).Age);
        }

        [Then(@"the (\d+) item should have Name equal to '(.*)'")]
        public void ItemInSetShouldHaveExpectedName(int itemNumber, string expectedName)
        {
            Assert.AreEqual(expectedName, GetItem(itemNumber).Name);
        }

        [Then(@"the (\d+) item should have LengthInMeters equal to '(\d+\.\d+)'")]
        public void ItemInSetShouldHaveExpectedLenghtInMeters(int itemNumber, double expectedLengthInMetersItem)
        {
            Assert.AreEqual(expectedLengthInMetersItem, GetItem(itemNumber).LengthInMeters);
        }

        [When(@"I create a set of dynamic instances from this table using no type conversion")]
        public void WhenICreateASetOfDynamicInstancesFromThisTableUsingNoTypeConversion(Table table)
        {
            State.OriginalSet = table.CreateDynamicSet(new Options { DoTypeConversion = false }).ToList();
        }
    }
}
