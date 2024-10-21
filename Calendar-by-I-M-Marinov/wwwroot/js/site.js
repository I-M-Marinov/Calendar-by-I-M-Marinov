// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Function to toggle date input types



// function to show dropdown on hover 
$(document).ready(function () {
    $(".nav-item .dropdown").hover(
        function () {
            $(this).find(".dropdown-menu").stop(true, true).delay(200).fadeIn(200);
        },
        function () {
            $(this).find(".dropdown-menu").stop(true, true).delay(200).fadeOut(200);
        }
    );
});

document.addEventListener('DOMContentLoaded', function () {
    var startDateInput = document.getElementById('startDateInput');
    var endDateInput = document.getElementById('endDateInput');
    var startDateLabel = document.getElementById('startDateLabel');
    var endDateLabel = document.getElementById('endDateLabel');
    var isAllDayCheckbox = document.getElementById('isAllDayCheckbox');
    var eventTypeSelect = document.getElementById('eventTypeSelect');

    // Get the initial values of start and end dates 
    var startDateValue = startDateInput.getAttribute('data-value') || startDateInput.value || '';
    var endDateValue = endDateInput.getAttribute('data-value') || endDateInput.value || '';

    function formatDate(dateString) {
        var date = new Date(dateString);
        // Adjust to local time zone
        var localDate = new Date(date.getTime() - (date.getTimezoneOffset() * 60000));
        return localDate.toISOString().slice(0, 16); // yyyy-MM-ddTHH:mm
    }

    function updateInputTypesAndLabels() {
        var isAllDay = isAllDayCheckbox.checked;
        var eventType = eventTypeSelect.value;

        if (isAllDay || eventType === 'allDay') {
            // Handle all-day events
            endDateInput.style.display = 'none';
            endDateLabel.style.display = 'none';
            startDateInput.type = 'date';
            startDateLabel.textContent = "Date";

            // Set input values formatted as "yyyy-MM-dd"
            if (startDateValue) {
                startDateInput.value = new Date(startDateValue).toISOString().slice(0, 10); // yyyy-MM-dd
            }
        } else {
            // Handle timed events
            startDateLabel.textContent = "Start";
            endDateInput.style.display = 'block';
            endDateLabel.style.display = 'block';
            startDateInput.type = 'datetime-local';
            endDateInput.type = 'datetime-local';

            // Set input values formatted as "yyyy-MM-ddTHH:mm"
            if (startDateValue) {
                startDateInput.value = formatDate(startDateValue); // yyyy-MM-ddTHH:mm
            }
            if (endDateValue) {
                endDateInput.value = formatDate(endDateValue); // yyyy-MM-ddTHH:mm
            }
        }
    }

    isAllDayCheckbox.addEventListener('change', updateInputTypesAndLabels);
    eventTypeSelect.addEventListener('change', updateInputTypesAndLabels);

    // Initial update on page load
    updateInputTypesAndLabels();
});





// FUNCTION TO ADD or REMOVE ATTENDANT'S INPUT FIELDS IN THE CreateEvent View


document.addEventListener('DOMContentLoaded', () => {
    const addAttendeeBtn = document.getElementById('addAttendee');
    const attendeesContainer = document.getElementById('attendeesContainer');

    // Make sure no duplicate event listeners are added
    if (addAttendeeBtn && !addAttendeeBtn.dataset.listenerAdded) {
        addAttendeeBtn.dataset.listenerAdded = true; // Flag to prevent duplicate listeners

        addAttendeeBtn.addEventListener('click', () => {
            const newAttendeeRow = document.createElement('div');
            newAttendeeRow.classList.add('attendee-row');

            newAttendeeRow.innerHTML = `
                <input type="email" name="Attendants" class="form-control" placeholder="Enter email address" />
                <button type="button" class="btn btn-remove-attendee">Remove</button>
            `;

            attendeesContainer.appendChild(newAttendeeRow);

            // Attach remove button functionality
            newAttendeeRow.querySelector('.btn-remove-attendee').addEventListener('click', () => {
                attendeesContainer.removeChild(newAttendeeRow);
            });
        });
    }

    // Attach remove button functionality for existing rows, ensure no duplicates
    document.querySelectorAll('.btn-remove-attendee').forEach(button => {
        if (!button.dataset.listenerAdded) {
            button.dataset.listenerAdded = true; // Prevent duplicate listeners

            button.addEventListener('click', () => {
                const row = button.parentElement;
                attendeesContainer.removeChild(row);
            });
        }
    });
});

// FUNCTION TO HANDLE THE GROUPS WHEN UPDATING A GROUP FOR A CONTACT ( ADDING OR REMOVING )

document.addEventListener('DOMContentLoaded', function () {
    const groupItems = document.querySelectorAll('.group-item');
    const selectedLabels = new Set();
    const selectedLabelsContainer = document.getElementById('selectedLabelsContainer');

    const existingLabels = selectedLabelsContainer.querySelectorAll('input[type="hidden"]');
    existingLabels.forEach(hiddenInput => {
        const value = hiddenInput.value;
        selectedLabels.add(value); // Add existing labels to the set
        const item = Array.from(groupItems).find(i => i.getAttribute('data-value') === value);
        if (item) {
            item.classList.add('selected'); // mark existing selected groups
        }
    });

    groupItems.forEach(item => {
        item.addEventListener('click', function () {
            const value = this.getAttribute('data-value');
            this.classList.toggle('selected'); 

            if (selectedLabels.has(value)) {

                selectedLabels.delete(value); // Remove from the set
                const hiddenInput = document.getElementById(`label-${value}`);
                if (hiddenInput) {
                    selectedLabelsContainer.removeChild(hiddenInput); 
                }
            } else {

                selectedLabels.add(value); // Add to the set

                const hiddenInput = document.createElement('input');
                hiddenInput.type = 'hidden';
                hiddenInput.name = 'Labels'; 
                hiddenInput.id = `label-${value}`; 
                hiddenInput.value = value; 
                selectedLabelsContainer.appendChild(hiddenInput); 
            }
        });
    });
});


// FUNCTIONS TO HANDLE THE POP-UP WHEN CREATING, REMOVING OR UPDATING A NEW LABEL / CONTACT GROUP

const cancelButtonAddElement = document.getElementById('modal-cancel-button1');
const cancelButtonRemoveElement = document.getElementById('modal-cancel-button2');
const cancelButtonUpdateElement = document.getElementById('modal-cancel-button3');

// For adding Label Modal
const addLabelModal = document.getElementById('addLabelModal');
const addLabelBtn = document.getElementById('addLabelButton');
const labelNameElement = document.getElementById('labelName');

addLabelBtn.onclick = function() {
    addLabelModal.style.display = 'block';
}

// For removing Label Modal
const removeLabelModal = document.getElementById('removeLabelModal');
const removeLabelBtn = document.getElementById('removeLabelButton');
const removeLabelSelectElement = document.getElementById('removeLabelSelect');

removeLabelBtn.onclick = function() {
    removeLabelModal.style.display = 'block';
}

// For editing Label Modal

const editLabelModal = document.getElementById('editLabelModal');
const editLabelBtn = document.getElementById('editLabelButton');
const updateLabelSelectElement = document.getElementById('updateLabelSelect');
const newGroupNameElement = document.getElementById('newGroupName');

editLabelBtn.onclick = function() {
    editLabelModal.style.display = 'block';
}

window.onclick = function (event) {
    if (event.target == addLabelModal) {
        addLabelModal.style.display = 'none';
    } else if (event.target == removeLabelModal) {
        removeLabelModal.style.display = 'none';
    } else if (event.target == editLabelModal) {
        editLabelModal.style.display = 'none';
    }
}

cancelButtonAddElement.addEventListener('click', function () {
    labelNameElement.value = '';  
    addLabelModal.style.display = 'none';  
});

cancelButtonRemoveElement.addEventListener('click', function () {
    removeLabelSelectElement.value = '';
    removeLabelModal.style.display = 'none';
});

cancelButtonUpdateElement.addEventListener('click', function () {
    updateLabelSelectElement.value = '';
    newGroupNameElement.value = '';
    editLabelModal.style.display = 'none';
});


function closePopup() {
    addLabelModal.style.display = 'none';
    removeLabelModal.style.display = 'none';
    editLabelModal.style.display = 'none';
}



// FUNCTION TO SET THE RETURN URL 

function submitDeleteForm(button) {
    var form = button.closest('form'); // Get the closest form to the button
    form.querySelector('#returnUrl').value = window.location.href; // Set returnUrl
    form.submit(); // Submit the form
}



// FUNCTION TO HIDE THE SUCCESS / ERROR MESSAGE 
document.addEventListener("DOMContentLoaded", function () {
    // Success Message
    var successMessage = document.getElementById('success-message');
    if (successMessage) {
        setTimeout(function() {
            successMessage.classList.add('fade-out');
        }, 1000);

        setTimeout(function() {
            successMessage.style.display = 'none';
        }, 3200);
    }

});
