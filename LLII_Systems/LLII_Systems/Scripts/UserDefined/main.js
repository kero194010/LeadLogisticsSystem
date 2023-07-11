$(document).ready(function () {
    //$(".sub-menu ul").hide();

   
    //pagination
    //$('#myTable').pageMe({ pagerSelector: '#myPager', showPrevNext: true, hidePageNumbers: false, perPage: 2 });
    //$('#table').pageMe({ pagerSelector: '#table', showPrevNext: true, hidePageNumbers: true, perPage: 50 });
 
    //ToastR
    var message = $('#Message').text();
    if (message !== '') {
        toastr.success(message);
    }
   
    var message1 = $('#Message1').text();
    if (message1 !== '') {
        toastr.success(message1);
    }
    var successMessage = $('#successMessage').text();
    if (successMessage !== '') {
        toastr.success(successMessage);
    }
    var errorMessage = $('#errorMessage').text();
    if (errorMessage !== '') {
        toastr.error(errorMessage);
    }


       //Item Detail functions
    var updateClicked = false;

    $('#updateButton').click(function () {
        if (updateClicked) {
            $('.enableButton').prop('disabled', true);
            updateClicked = false;
        } else {
            $('.enableButton').prop('disabled', false);
            updateClicked = true;
        }
    });

    $('#confirmSaveButton').click(function () {
        $('form').submit();
        $('#confirmationModal').modal('hide');
    });

    $('#myBtn').click(function (event) {
        $('#confirmationModal').modal('show');
        event.preventDefault(); // Prevent the default form submission
    });

  
    $('#addVendorBtn').click(function (event) {
        $('#vendorModal').modal('show');

    });

    $('#Test').click(function (event) {
        $('#myModals').modal('hide');
        alert('PASOk')

    });
});


function showModal() {
    $('body').loadingModal({
        position: 'auto',
        text: '',
        color: '#fff',
        opacity: '0.7',
        backgroundColor: 'rgb(0,0,0)',
        animation: 'wanderingCubes'
    });
 
}