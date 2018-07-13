using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using IOMappingWebApi.Controllers;
using IOMappingWebApi.Model;
using Xunit;
using System;
using Moq;
using System.Diagnostics;

namespace IOMappingWebApi_Tests
{
    public class InstanceControllerTest
    {
        [Fact]
        public void GetParticular()
        {
            // Arrange
            var mock = new Mock<IGalaxyObject_UoW>();
            mock.SetupGet(m => m.Contents.EntityCollection)
            .Returns(new List<InstanceContent>
            {
                new InstanceContent(){
                    Instance = new Instance(){ Name = "Valve01" },
                    Attribute = new IOMappingWebApi.Model.Attribute(){ Name = "RAW" },
                    IOTag = new IOTag(){ Name = "IO1001" }, PLCTag = new PLCTag(){ Name = "PLC1001" }
                },
                    new InstanceContent(){
                    Instance = new Instance(){ Name = "Valve01" },
                    Attribute = new IOMappingWebApi.Model.Attribute(){ Name = "Reading1" },
                    IOTag = new IOTag(){ Name = "IO1005" }, PLCTag = new PLCTag(){ Name = "PLC1005" }
                },
                    new InstanceContent(){
                    Instance = new Instance(){ Name = "Valve01" },
                    Attribute = new IOMappingWebApi.Model.Attribute(){ Name = "Reading2" },
                    IOTag = new IOTag(){ Name = "IO1004" }, PLCTag = new PLCTag(){ Name = "PLC1007" }
                }
            });


            //var controller = new ObjectController(mock.Object);
            //controller.Post(mock.Object.Contents.EntityCollection);

            // Act
            //var c_result = controller.Get("FirstID") as ContentResult;
            //GalaxyObject GObj = (GalaxyObject)Newtonsoft.Json.JsonConvert.DeserializeObject<GalaxyObject>(c_result.Content);

            Assert.Equal(mock.Object.Contents.EntityCollection, null);
            //Assert.Equal(mock.Object.Contents[0].PLCTag.Name, GObj.Content[0].PLCTag.Name);
            //Assert.Equal(mock.Object.Contents[0].IOTag.Name, GObj.Content[0].IOTag.Name);
            //Assert.Equal(2, mock.Object.Contents.Count);

        }

        [Fact]
        public void PostContent()
        {
            // Arrange
            //var mock = new Mock<IAttribute_Repository>();
            //mock.SetupGet(m => m.Attributes)
            //.Returns(new List<IOMappingWebApi.Model.Attribute>
            //{
            //    new IOMappingWebApi.Model.Attribute(){ ID = 1, Name = "RAW" },
            //    new IOMappingWebApi.Model.Attribute(){ ID = 2, Name = "RAW2" },
            //    new IOMappingWebApi.Model.Attribute(){ ID = 3, Name = "RAW3" },
            //});


            // Act
            //var Attribute_Ctrl = new AttributeController(mock.Object);
            AttributeController Attribute_Ctrl = new AttributeController(new Attribute_Repository(new GalaxyObjectContext()));
            //Attribute_Ctrl.PushToDatabase(new List<IOMappingWebApi.Model.Attribute>
            //{
            //    new IOMappingWebApi.Model.Attribute() { Name = "RAW" }, 
            //    new IOMappingWebApi.Model.Attribute() { Name = "Meters" },
            //    new IOMappingWebApi.Model.Attribute() { Name = "Meters2" },
            //    new IOMappingWebApi.Model.Attribute() { Name = "RAW3" },
            //    new IOMappingWebApi.Model.Attribute() { Name = "Meters3" },
            //});



            // Assert
            //Assert.Equal(mock.Object.Attributes, null);

            //foreach (IOMappingWebApi.Model.Attribute Att in mock.Object.Attributes)
            //{ 
            //}

        }



    }
}

