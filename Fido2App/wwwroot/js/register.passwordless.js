document.getElementById('registerSubmit').addEventListener('click', handleRegisterSubmit);

async function handleRegisterSubmit(event) {
    event.preventDefault();
    let username = document.getElementById('Username').value;
    let displayName = document.getElementById('DisplayName').value;
    
    // possible values: none, direct, indirect
    let attestation_type = "none";
    // possible values: <empty>, platform, cross-platform
    let authenticator_attachment = "cross-platform";

    // possible values: preferred, required, discouraged
    let user_verification = "preferred";

    // possible values: true,false
    let require_resident_key = true;
    
    let requestData = {
        username : username,
        displayName: displayName,
        attestationType : attestation_type, 
        authenticatorAttachment : authenticator_attachment,
        userVerification : user_verification,
        requireResidentKey: require_resident_key
    }

    // send to server for registering
    let makeCredentialOptions;
    try {
        makeCredentialOptions = await fetchMakeCredentialOptions(requestData);
        
    } catch (e) {
        console.error(e);
        let msg = "Something wen't really wrong";
        showErrorAlert(msg);
    }
    
    if (!makeCredentialOptions?.isSuccess){
        showErrorAlert(makeCredentialOptions?.message);
        return;
    }

    console.log("Credential Options Object", makeCredentialOptions);

    if (makeCredentialOptions?.status !== "ok") {
        console.log("Error creating credential options");
        console.log(makeCredentialOptions?.errorMessage);
        showErrorAlert(makeCredentialOptions?.errorMessage);
        return;
    }

    // Turn the challenge back into the accepted format of padded base64
    makeCredentialOptions.challenge = coerceToArrayBuffer(makeCredentialOptions.challenge);
    // Turn ID into a UInt8Array Buffer for some reason
    makeCredentialOptions.user.id = coerceToArrayBuffer(makeCredentialOptions.user.id);

    // Turn the challenge back into the accepted format of padded base64
    makeCredentialOptions.challenge = coerceToArrayBuffer(makeCredentialOptions.challenge);
    // Turn ID into a UInt8Array Buffer for some reason
    makeCredentialOptions.user.id = coerceToArrayBuffer(makeCredentialOptions.user.id);

    makeCredentialOptions.excludeCredentials = makeCredentialOptions.excludeCredentials.map((c) => {
        c.id = coerceToArrayBuffer(c.id);
        return c;
    });

    if (makeCredentialOptions.authenticatorSelection.authenticatorAttachment === null) 
        makeCredentialOptions.authenticatorSelection.authenticatorAttachment = undefined;

    console.log("Credential Options Formatted", makeCredentialOptions);

    Swal.fire({
        title: 'Registering...',
        text: 'Tap your security key to finish registration.',
        imageUrl: "/images/securitykey.min.svg",
        showCancelButton: true,
        showConfirmButton: false,
        focusConfirm: false,
        focusCancel: false
    });


    console.log("Creating PublicKeyCredential...");

    let newCredential;
    try {
        newCredential = await navigator.credentials.create({publicKey: makeCredentialOptions}); // qr burda olusuyor
    } catch (e) {
        let msg = "Could not create credentials in browser. Probably because the username is already registered with your authenticator. Please change username or authenticator."
        console.error(msg, e);
        showErrorAlert(msg, e);
    }


    console.log("PublicKeyCredential Created", newCredential);

    try {
       await registerNewCredential(newCredential);

    } catch (e) {
        showErrorAlert(e.message ? e.message : e);
    }
    

}

async function fetchMakeCredentialOptions(requestData) {
    let response = await fetch('/makeCredentialOptions', {
        method: 'POST', // or 'PUT'
        body: JSON.stringify(requestData), // data can be `string` or {object}!
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    });
    
    return await response.json();
}

// This should be used to verify the auth data with the server
async function registerNewCredential(newCredential) {
    // Move data into Arrays incase it is super long
    let attestationObject = new Uint8Array(newCredential.response.attestationObject);
    let clientDataJSON = new Uint8Array(newCredential.response.clientDataJSON);
    let rawId = new Uint8Array(newCredential.rawId);

    const data = {
        id: newCredential.id,
        rawId: coerceToBase64Url(rawId),
        type: newCredential.type,
        extensions: newCredential.getClientExtensionResults(),
        response: {
            AttestationObject: coerceToBase64Url(attestationObject),
            clientDataJSON: coerceToBase64Url(clientDataJSON),
        },
    };

    let response;
    try {
        response = await registerCredentialWithServer(data);
    } catch (e) {
        showErrorAlert(e);
    }

    if (!response?.isSuccess){
        showErrorAlert(response?.message);
        return;
    }

    console.log("Credential Object", response);

    // show error
    if (response?.status !== "ok") {
        console.log("Error creating credential");
        console.log(response?.errorMessage);
        showErrorAlert(response?.errorMessage);
        return;
    }

    // show success 
    Swal.fire({
        title: 'Registration Successful!',
        text: 'You\'ve registered successfully.',
        type: 'success',
        timer: 2000
    });
}

async function registerCredentialWithServer(formData) {
    let response = await fetch('/makeCredential', {
        method: 'POST', // or 'PUT'
        body: JSON.stringify({
            authenticatorAttestationRawResponse : formData
        }), // data can be `string` or {object}!
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    });

    return await response.json();
}