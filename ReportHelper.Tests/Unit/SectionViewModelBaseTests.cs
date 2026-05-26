using ReportHelper.ViewModels.Base;

namespace ReportHelper.Tests.Unit
{
    public class SectionViewModelBaseTests
    {
        [Fact]
        public void AllRequiredFieldsFilled_OnConfirm_CanAdvanceIsTrueAndEventFires()
        {
            //Arrange
            var viewModel = new SectionViewModelBase();
            bool eventFired = false;
            viewModel.SectionAdvanced += (sender, args) => eventFired = true;
            viewModel.RequiredFields.Add("Officer Name", "John Doe");

            //Act
            viewModel.OnConfirm();

            //Assert
            Assert.True(viewModel.CanAdvance);
            Assert.True(eventFired);
        }

        [Fact]
        public void MissingRequiredField_OnConfirm_CanAdvanceIsFalseAndErrorMessageSet()
        {
            //Arrange
            var viewModel = new SectionViewModelBase();
            bool eventFired = false;
            viewModel.SectionAdvanced += (sender, args) => eventFired = true;
            viewModel.RequiredFields.Add("Officer Name", ""); // missing required field

            //Act
            viewModel.OnConfirm();

            //Assert
            Assert.False(viewModel.CanAdvance);
            Assert.Equal("Please fill out the required field: Officer Name", viewModel.ErrorMessage);
            Assert.False(eventFired);
        }

        [Fact]
        public void NoRequiredFieldsFilled_OnConfirm_CanAdvanceIsTrue() 
        {
            //Arrange
            var viewModel = new SectionViewModelBase();

            //Act
            viewModel.OnConfirm();

            //Assert
            Assert.True(viewModel.CanAdvance);
        }
    }
}
