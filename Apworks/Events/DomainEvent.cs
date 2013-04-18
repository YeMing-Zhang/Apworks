﻿// ==================================================================================================================                                                                                          
//        ,::i                                                           BBB                
//       BBBBBi                                                         EBBB                
//      MBBNBBU                                                         BBB,                
//     BBB. BBB     BBB,BBBBM   BBB   UBBB   MBB,  LBBBBBO,   :BBG,BBB :BBB  .BBBU  kBBBBBF 
//    BBB,  BBB    7BBBBS2BBBO  BBB  iBBBB  YBBJ :BBBMYNBBB:  FBBBBBB: OBB: 5BBB,  BBBi ,M, 
//   MBBY   BBB.   8BBB   :BBB  BBB .BBUBB  BB1  BBBi   kBBB  BBBM     BBBjBBBr    BBB1     
//  BBBBBBBBBBBu   BBB    FBBP  MBM BB. BB BBM  7BBB    MBBY .BBB     7BBGkBB1      JBBBBi  
// PBBBFE0GkBBBB  7BBX   uBBB   MBBMBu .BBOBB   rBBB   kBBB  ZBBq     BBB: BBBJ   .   iBBB  
//BBBB      iBBB  BBBBBBBBBE    EBBBB  ,BBBB     MBBBBBBBM   BBB,    iBBB  .BBB2 :BBBBBBB7  
//vr7        777  BBBu8O5:      .77r    Lr7       .7EZk;     L77     .Y7r   irLY  JNMMF:    
//               LBBj
//
// Apworks Application Development Framework
// Copyright (C) 2010-2011 apworks.codeplex.com.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ==================================================================================================================

using Apworks.Application;
using Apworks.Bus;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Apworks.Events
{
    /// <summary>
    /// Represents the base class for all domain events.
    /// </summary>
    [Serializable]
    public abstract class DomainEvent : IDomainEvent
    {
        #region Private Static Fields
        private static readonly IMessageDispatcher messageDispatcher;
        #endregion

        #region Ctor
        /// <summary>
        /// Make static initialization of the <c>DomainEvent</c> class.
        /// </summary>
        static DomainEvent()
        {
            if (ServiceLocator.Instance.Registered<IMessageDispatcher>())
                messageDispatcher = ServiceLocator.Instance.GetService<IMessageDispatcher>();
            else
                messageDispatcher = MessageDispatcher.CreateAndRegister(AppRuntime.Instance.CurrentApplication.ConfigSource,
                    typeof(MessageDispatcher));
        }
        /// <summary>
        /// Initializes a new instance of <c>DomainEvent</c> class.
        /// </summary>
        public DomainEvent() { }
        /// <summary>
        /// Initializes a new instace of <c>DomainEvent</c> class.
        /// </summary>
        /// <param name="source">The source entity which raises the domain event.</param>
        public DomainEvent(IEntity source)
        {
            this.Source = source;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the hash code for current domain event.
        /// </summary>
        /// <returns>The calculated hash code for the current domain event.</returns>
        public override int GetHashCode()
        {
            return Utils.GetHashCode(this.Source.GetHashCode(),
                this.Branch.GetHashCode(),
                this.ID.GetHashCode(),
                this.Timestamp.GetHashCode(),
                this.Version.GetHashCode());
        }
        /// <summary>
        /// Returns a <see cref="System.Boolean"/> value indicating whether this instance is equal to a specified
        /// entity.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>True if obj is an instance of the <see cref="Apworks.ISourcedAggregateRoot"/> type and equals the value of this
        /// instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            if (obj == null)
                return false;
            DomainEvent other = obj as DomainEvent;
            if ((object)other == (object)null)
                return false;
            return this.ID == other.ID;
        }
        #endregion

        #region IDomainEvent Members
        /// <summary>
        /// Gets or sets the identifier of the aggregate root.
        /// </summary>
        // public virtual Guid SourceID { get; set; }
        /// <summary>
        /// Gets or sets the assembly qualified name of the type of the aggregate root.
        /// </summary>
        // public virtual string AssemblyQualifiedSourceType { get; set; }
        /// <summary>
        /// Gets or sets the source of the entity which raises the domain event.
        /// </summary>
        [XmlIgnore]
        [SoapIgnore]
        [IgnoreDataMember]
        public IEntity Source
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the version of the domain event.
        /// </summary>
        public virtual long Version { get; set; }
        /// <summary>
        /// Gets or sets the branch on which the current version of domain event exists.
        /// </summary>
        public virtual long Branch { get; set; }
        /// <summary>
        /// Gets or sets the assembly qualified type name of the event.
        /// </summary>
        public virtual string AssemblyQualifiedEventType { get; set; }
        #endregion

        #region IEvent Members
        /// <summary>
        /// Gets or sets the date and time on which the event was produced.
        /// </summary>
        /// <remarks>The format of this date/time value could be various between different
        /// systems. Apworks recommend system designer or architect uses the standard
        /// UTC date/time format.</remarks>
        public virtual DateTime Timestamp { get; set; }
        #endregion

        #region IEntity Members
        /// <summary>
        /// Gets or sets the identifier of the domain event.
        /// </summary>
        public virtual Guid ID { get; set; }
        #endregion

        #region Public Static Methods

        public static void Subscribe<TDomainEvent>(IDomainEventHandler<TDomainEvent> domainEventHandler)
            where TDomainEvent : IDomainEvent
        {
            messageDispatcher.Register<TDomainEvent>(domainEventHandler);
        }

        public static void Unsubscribe<TDomainEvent>(IDomainEventHandler<TDomainEvent> domainEventHandler)
            where TDomainEvent : IDomainEvent
        {
            messageDispatcher.UnRegister<TDomainEvent>(domainEventHandler);
        }

        public static void Publish<TDomainEvent>(TDomainEvent @event)
            where TDomainEvent : IDomainEvent
        {
            messageDispatcher.DispatchMessage<TDomainEvent>(@event);
        }

        public static void Publish<TDomainEvent>(TDomainEvent @event, Action<TDomainEvent, Exception> callback)
            where TDomainEvent : IDomainEvent
        {
            Exception exception = null;
            try
            {
                messageDispatcher.DispatchMessage<TDomainEvent>(@event);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            callback(@event, exception);
        }
        #endregion
    }
}
