class StudentClass:
    """description of class"""
    studentCount="11"
    
    def __init__(self, name, sex):
        self.name=name
        self.sex=sex
        studentCount=name

    def displayCount(self):
        print("Total student is:{0}".format(StudentClass.studentCount))

    def displayEmployee(self):
      print ("Name : ", self.name,  ", sex: ", self.sex)



