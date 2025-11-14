#!/usr/bin/env python3
"""
Test JSON parsing logic for API responses
"""
import json

# Sample POST response
post_response = {
    "success": True,
    "data": {
        "submissionId": "b67dec2e-3893-4ae2-8f86-7576a3d76ad6",
        "status": "Running",
        "submittedAt": "2025-11-09T16:57:34.6356166+00:00"
    },
    "message": "Judging in progress",
    "errors": None
}

# Sample GET response - Pending
get_response_pending = {
    "success": True,
    "data": {
        "sourceCodeRef": "string",
        "language": "string",
        "status": "Pending",
        "compareResult": "",
        "errorCode": "",
        "errorMessage": "",
        "totalTime": 0,
        "totalMemory": 0,
        "resultFileRef": "string",
        "submittedAt": "2024-01-15T10:30:00Z"
    },
    "message": "string",
    "errors": []
}

# Sample GET response - Running
get_response_running = {
    "success": True,
    "data": {
        "sourceCodeRef": "string",
        "language": "cpp",
        "status": "Running",
        "compareResult": "",
        "errorCode": "",
        "errorMessage": "",
        "totalTime": 150,
        "totalMemory": 3500,
        "resultFileRef": "string",
        "submittedAt": "2024-01-15T10:30:00Z"
    },
    "message": "Processing testcases",
    "errors": []
}

# Sample GET response - Passed
get_response_passed = {
    "success": True,
    "data": {
        "sourceCodeRef": "string",
        "language": "cpp",
        "status": "Passed",
        "compareResult": "000000000000",  # All testcases passed
        "errorCode": "",
        "errorMessage": "",
        "totalTime": 450,
        "totalMemory": 52000,
        "resultFileRef": "string",
        "submittedAt": "2024-01-15T10:30:00Z"
    },
    "message": "All testcases passed",
    "errors": []
}

# Sample GET response - Failed
get_response_failed = {
    "success": True,
    "data": {
        "sourceCodeRef": "string",
        "language": "cpp",
        "status": "Failed",
        "compareResult": "0001000000",  # Some testcases failed
        "errorCode": "WrongAnswer",
        "errorMessage": "Testcase #4 - WrongAnswer: Expected: 3 | Got: 2",
        "totalTime": 250,
        "totalMemory": 35000,
        "resultFileRef": "string",
        "submittedAt": "2024-01-15T10:30:00Z"
    },
    "message": "Some testcases failed",
    "errors": []
}

def test_parsing(response_data, description):
    """Test parsing logic"""
    print(f"\n{'='*70}")
    print(f"Testing: {description}")
    print(f"{'='*70}")
    
    if response_data.get('success'):
        data = response_data.get('data', {})
        status = data.get('status', 'Unknown')
        
        print(f"✅ Success: {response_data.get('success')}")
        print(f"📊 Status: {status}")
        print(f"💬 Message: {response_data.get('message')}")
        
        # Check fields
        if 'submissionId' in data:
            print(f"🎯 Submission ID: {data['submissionId']}")
        
        total_time = data.get('totalTime', 0)
        total_memory = data.get('totalMemory', 0)
        
        if total_time > 0 or total_memory > 0:
            print(f"⏱️  Time: {total_time}ms, Memory: {total_memory}KB")
        
        compare_result = data.get('compareResult', '')
        if compare_result:
            print(f"📝 Compare Result: {compare_result}")
        
        error_message = data.get('errorMessage', '')
        if error_message:
            print(f"❌ Error: {error_message}")
        
        # Check if should continue polling
        if status in ['Passed', 'Failed']:
            print(f"\n🛑 STOP POLLING - Final status reached: {status}")
            return False  # Stop polling
        else:
            print(f"\n🔄 CONTINUE POLLING - Current status: {status}")
            return True  # Continue polling
    else:
        print(f"❌ API Error: {response_data.get('message')}")
        print(f"Errors: {response_data.get('errors')}")
        return False

if __name__ == "__main__":
    print("🧪 Testing JSON Parsing Logic")
    
    # Test POST response
    test_parsing(post_response, "POST Response (Initial Submit)")
    
    # Test GET responses
    test_parsing(get_response_pending, "GET Response - Pending")
    test_parsing(get_response_running, "GET Response - Running")
    test_parsing(get_response_passed, "GET Response - Passed (FINAL)")
    test_parsing(get_response_failed, "GET Response - Failed (FINAL)")
    
    print("\n" + "="*70)
    print("✅ All parsing tests completed!")
    print("="*70)
