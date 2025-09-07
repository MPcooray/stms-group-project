export const required = v => (v ? '' : 'This field is required')
export const email = v => (/\S+@\S+\.\S+/.test(v) ? '' : 'Enter a valid email')
