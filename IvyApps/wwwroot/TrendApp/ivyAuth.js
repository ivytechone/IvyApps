const authorizeEndpoint = "https://auth.ivytech.one/authorize";
const clientId = "5967CA44-7FE7-462E-8DC3-1E2024F664AA";

function onLogin() {
    const state = genRandomBase64(4);
    const code_verifier = genRandomBase64(8);

    getSHA256Hash(code_verifier).then((code_challenge) => {
        window.sessionStorage.setItem('authState', state);
        window.sessionStorage.setItem('authCodeVerifier', code_verifier);
        window.location = `${authorizeEndpoint}?response_type=code&client_id=${clientId}&state=${encodeURIComponent(state)}&scopes=default&code_challenge=${code_challenge}&redirect_uri=https://${window.location.host}/auth`;
    })
}

const genRandomBase64 = (len) => {
    const array = new Uint32Array(len);
    crypto.getRandomValues(array);
    return btoa(array);
}

const getSHA256Hash = async (input) => {
    const textAsBuffer = new TextEncoder().encode(input);
    const hashBuffer = await window.crypto.subtle.digest("SHA-256", textAsBuffer);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    const hash = hashArray
        .map((item) => item.toString(16).padStart(2, "0"))
        .join("");
    return hash;
};

const exchangeToken = async () => {
    const authState = window.sessionStorage.getItem('authState');
    const authCodeVerifier = window.sessionStorage.getItem('authCodeVerifier');
    const urlParams = new URLSearchParams(window.location.search);
    const code = urlParams.get('code');
    const state = urlParams.get('state');

    if (authState && code && state && state === authState) {
        var body = {
            code,
            code_verifier: authCodeVerifier
        };

        const response = await fetch(`/token`, {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: "same-origin",
            body: JSON.stringify(body),
        });

        if (response.ok) {
            window.localStorage.setItem('auth', 'true');
            window.location.href = "/"
            return true;
        }
    }

    return false;
}