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

        // ── BL-10 — ActiveDictationField ───────────────────────────────────────
        //
        // Problem this solves: ReportHeaderView has 3 voice-enabled fields
        // (Officer Name, Badge Number, Unit/Division) sharing this one base
        // class's single DictatedText property. Without knowing WHICH field's
        // Dictate button was pressed, a derived ViewModel has no way to route
        // a finished transcription into the right property. ActiveDictationField
        // is set by StartRecording(fieldName) and read by the derived class's
        // own OnDictatedTextChanged hook (added per-ViewModel, not here — this
        // base class only needs to remember the name, not know what to do with it).

        [Fact]
        public void StartRecording_SetsActiveDictationField_ToGivenFieldName()
        {
            // Arrange
            var viewModel = new SectionViewModelBase();

            // Act
            viewModel.StartRecording("OfficerName");

            // Assert
            Assert.Equal("OfficerName", viewModel.ActiveDictationField);
        }

        [Fact]
        public void StartRecording_CalledAgainWithDifferentField_OverwritesActiveDictationField()
        {
            // Arrange — simulates the officer dictating into one field, then
            // pressing a different field's Dictate button next.
            var viewModel = new SectionViewModelBase();
            viewModel.StartRecording("OfficerName");

            // Act
            viewModel.StartRecording("BadgeNumber");

            // Assert
            Assert.Equal("BadgeNumber", viewModel.ActiveDictationField);
        }

        [Fact]
        public void ActiveDictationField_DefaultsToEmptyString()
        {
            // Arrange + Act
            var viewModel = new SectionViewModelBase();

            // Assert — matches the existing string-field convention (empty,
            // never null) used by every other string property in this class.
            Assert.Equal(string.Empty, viewModel.ActiveDictationField);
        }
    }
}
