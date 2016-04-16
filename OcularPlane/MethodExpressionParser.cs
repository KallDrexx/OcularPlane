using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OcularPlane.InternalModels;
using OcularPlane.Models;

namespace OcularPlane
{
    static class MethodExpressionParser
    {
        public static TrackedMethod ParseToReference(Expression<Action> methodExpression, string storedName)
        {
            if (methodExpression == null) throw new ArgumentNullException(nameof(methodExpression));
            if (string.IsNullOrWhiteSpace(storedName)) throw new ArgumentNullException(storedName);

            var methodCall = methodExpression.Body as MethodCallExpression;
            if (methodCall == null)
            {
                throw new ArgumentException("Expression is not a method");
            }

            var methodInfo = methodCall.Method;
            var attachedObject = GetClassObject(methodCall);
            if (!methodInfo.IsStatic && attachedObject == null)
            {
                var message = @"Could not find object method is attached to.  Only static method calls, or calls with an explicit instance declared
are allowed (e.g. ""() => instance.MethodCall()"".  ""this.MethodCall()"", ""MethodCall()"", and other variations are not supported";

                throw new InvalidOperationException(message);
            }

            ValidateMethodParameters(methodInfo);

            var methodId = Guid.NewGuid();
            return CreateTrackedMethod(storedName, methodId, attachedObject, methodInfo);
        }

        private static TrackedMethod CreateTrackedMethod(string storedName, Guid methodId, object attachedObject, MethodInfo methodInfo)
        {
            return new TrackedMethod
            {
                MethodId = methodId,
                RelvantObject = attachedObject,
                MethodInfo = methodInfo,
                Reference = new MethodReference
                {
                    MethodId = methodId,
                    Name = storedName,
                    Parameters = methodInfo.GetParameters()
                        .Select(x => new ParameterReference
                        {
                            TypeName = x.ParameterType.FullName,
                            ParameterName = x.Name
                        })
                        .ToArray()
                }
            };
        }

        private static object GetClassObject(MethodCallExpression methodCall)
        {
            var member = methodCall.Object as MemberExpression;
            var constant = member?.Expression as ConstantExpression;
            if (constant == null)
            {
                return null;
            }

            var anonymousClassInstance = constant.Value;
            var calledClassField = (FieldInfo)member.Member;
            
            return calledClassField.GetValue(anonymousClassInstance);
        }

        private static void ValidateMethodParameters(MethodInfo method)
        {
            var validations = new List<Func<ParameterInfo, bool>>
            {
                p => typeof (IConvertible).IsAssignableFrom(p.ParameterType)
            };

            var validationsPassed = method.GetParameters()
                .Select(parameterInfo => validations.Any(validationFunction => validationFunction(parameterInfo)))
                .All(x => x);

            if (!validationsPassed)
            {
                throw new InvalidOperationException("Parameters of methods being added to containers must implement IConvertible");
            }
        }
    }
}
