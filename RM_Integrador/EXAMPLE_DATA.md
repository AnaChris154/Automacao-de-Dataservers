# 📊 Example Data

## Employee Data Example

### GET Request
```json
{
  "filter": "DEPARTMENT = 'IT' AND ACTIVE = 1",
  "fields": ["ID", "NAME", "POSITION", "HIRE_DATE"]
}
```

### Response
```json
{
  "success": true,
  "data": [
    {
      "ID": "EMP001",
      "NAME": "John Smith",
      "POSITION": "Senior Developer",
      "HIRE_DATE": "2023-01-15"
    },
    {
      "ID": "EMP002",
      "NAME": "Jane Doe",
      "POSITION": "Project Manager",
      "HIRE_DATE": "2022-06-30"
    }
  ]
}
```

## Department Data Example

### GET Request
```json
{
  "filter": "LOCATION = 'HQ'",
  "fields": ["DEPT_ID", "DEPT_NAME", "MANAGER_NAME"]
}
```

### Response
```json
{
  "success": true,
  "data": [
    {
      "DEPT_ID": "IT01",
      "DEPT_NAME": "Information Technology",
      "MANAGER_NAME": "John Smith"
    },
    {
      "DEPT_ID": "HR01",
      "DEPT_NAME": "Human Resources",
      "MANAGER_NAME": "Sarah Johnson"
    }
  ]
}
```

## JSON Comparison Example

### Request
```json
{
  "source": {
    "employee": {
      "id": "EMP001",
      "name": "John Smith",
      "department": "IT",
      "salary": 75000
    }
  },
  "target": {
    "employee": {
      "id": "EMP001",
      "name": "John Smith",
      "department": "Development",
      "position": "Senior Developer"
    }
  }
}
```

### Response
```json
{
  "differences": [
    {
      "path": "employee.department",
      "sourceValue": "IT",
      "targetValue": "Development",
      "type": "value-mismatch"
    },
    {
      "path": "employee.salary",
      "sourceValue": 75000,
      "targetValue": null,
      "type": "field-missing-in-target"
    },
    {
      "path": "employee.position",
      "sourceValue": null,
      "targetValue": "Senior Developer",
      "type": "field-missing-in-source"
    }
  ]
}
```