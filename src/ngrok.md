ref https://docs.microsoft.com/en-us/azure/azure-functions/functions-debug-event-grid-trigger-local

#run ngrok
ngrok http -host-header=localhost 7074

#from local to ngrok webhook
http://localhost:7074/runtime/webhooks/EventGrid?functionName=fx_sendgrid
https://e1f2ec63.ngrok.io/runtime/webhooks/EventGrid?functionName=fx_sendgrid

#sendgrid activity
https://app.sendgrid.com/email_activity
